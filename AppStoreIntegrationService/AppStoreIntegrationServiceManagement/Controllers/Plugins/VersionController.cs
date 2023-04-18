using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceManagement.DataBase.Interface;
using AppStoreIntegrationServiceManagement.Filters;
using AppStoreIntegrationServiceManagement.Helpers;
using AppStoreIntegrationServiceManagement.Model;
using AppStoreIntegrationServiceManagement.Model.Notifications;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppStoreIntegrationServiceManagement.Controllers.Plugins
{
    [Area("Plugins")]
    [Authorize]
    [DBSynched]
    [AccountSelect]
    [TechPartnerAgreement]
    public class VersionController : CustomController
    {
        private readonly IPluginRepository _pluginRepository;
        private readonly IProductsRepository _productsRepository;
        private readonly IPluginVersionRepository _pluginVersionRepository;
        private readonly ILoggingRepository _loggingRepository;
        private readonly INotificationCenter _notificationCenter;

        public VersionController
        (
            IUserProfilesManager userProfilesManager,
            IUserAccountsManager userAccountsManager,
            IPluginRepository pluginRepository,
            IProductsRepository productsRepository,
            ILoggingRepository loggingRepository,
            IPluginVersionRepository pluginVersionRepository,
            INotificationCenter notificationCenter,
            IAccountsManager accountsManager
        ) : base(userProfilesManager, userAccountsManager, accountsManager)
        {
            _pluginRepository = pluginRepository;
            _productsRepository = productsRepository;
            _loggingRepository = loggingRepository;
            _pluginVersionRepository = pluginVersionRepository;
            _notificationCenter = notificationCenter;
        }

        [Route("/Plugins/Edit/{pluginId}/Versions")]
        public async Task<IActionResult> Index(int pluginId)
        {
            var extendedVersions = new List<ExtendedPluginVersion>();
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            var extendedPlugin = ExtendedPluginDetails.CopyFrom(plugin);
            extendedPlugin.IsEditMode = true;

            foreach (var version in await _pluginVersionRepository.GetPluginVersions(pluginId))
            {
                var extendedVersion = ExtendedPluginVersion.CopyFrom(version);
                extendedVersion.PluginId = pluginId;
                extendedVersions.Add(extendedVersion);
            }

            return View((extendedPlugin, extendedVersions.Where(v => IsEligibleVersion(v))));
        }

        [Route("/Plugins/Edit/{pluginId}/Versions/Edit/{versionId}")]
        public async Task<IActionResult> Edit(int pluginId, string versionId) => await Render(pluginId, versionId, Status.Active, Pending, "Details");

        [Route("/Plugins/Edit/{pluginId}/Versions/Pending/{versionId}")]
        public async Task<IActionResult> Pending(int pluginId, string versionId) => await Render(pluginId, versionId, Status.InReview, Draft, "Pending");

        [Route("/Plugins/Edit/{pluginId}/Versions/Draft/{versionId}")]
        public async Task<IActionResult> Draft(int pluginId, string versionId) => await Render(pluginId, versionId, Status.Draft, Edit, "Draft");

        [Route("/Plugins/Edit/{pluginId}/Versions/Add")]
        public async Task<IActionResult> Add(int pluginId)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            var products = await _productsRepository.GetAllProducts();
            var extendedPlugin = ExtendedPluginDetails.CopyFrom(plugin);
            var versions = await _pluginVersionRepository.GetPluginVersions(pluginId);

            extendedPlugin.IsEditMode = true;
            extendedPlugin.Parents = await _productsRepository.GetAllParents();
            extendedPlugin.Versions = versions.Where(v => v.IsThirdParty && ExtendedUser.IsInRole("Developer") || ExtendedUser.IsInRole("SystemAdministrator") && v.HasAdminConsent).ToList();

            var extendedVersion = new ExtendedPluginVersion
            {
                VersionId = Guid.NewGuid().ToString(),
                SupportedProductsListItems = new MultiSelectList(products, nameof(ProductDetails.Id), nameof(ProductDetails.ProductName)),
                IsNewVersion = true,
                PluginId = pluginId,
                VersionStatus = Status.Draft,
                IsThirdParty = plugin.IsThirdParty
            };

            return View("Details", (extendedPlugin, extendedVersion));
        }

        [HttpPost]
        public async Task<IActionResult> GenerateChecksum(string downloadUrl)
        {
            try
            {
                var remoteReader = new RemoteStreamReader(new Uri(downloadUrl));
                var filehash = SHA1Generator.GetHash(await remoteReader.ReadAsStreamAsync());
                TempData["Filehash"] = filehash;
                return PartialView("_StatusMessage", "Success! Checksum was generated!");
            }
            catch (Exception e)
            {
                return PartialView("_StatusMessage", $"Error! {e.Message}");
            }
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Activate")]
        [RoleAuthorize("SystemAdministrator")]
        public async Task<IActionResult> Activate(int pluginId, PluginVersion version)
        {
            version.VersionStatus = Status.Active;
            await _loggingRepository.Log(new Log
            {
                Author = ExtendedUser.AccountName,
                Description = TemplateResource.VersionActiveLog,
                TargetInfo = version.VersionNumber
            }, pluginId);
            return await Save(pluginId, version, "Edit");
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Deactivate")]
        [RoleAuthorize("SystemAdministrator")]
        public async Task<IActionResult> Deactivate(int pluginId, PluginVersion version)
        {
            version.VersionStatus = Status.Inactive;
            await _loggingRepository.Log(new Log
            {
                Author = ExtendedUser.AccountName,
                Description = TemplateResource.VersionInactiveLog,
                TargetInfo = version.VersionNumber
            }, pluginId);
            return await Save(pluginId, version, "Edit");
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Submit")]
        [RoleAuthorize("Developer", "Administrator")]
        public async Task<IActionResult> Submit(int pluginId, PluginVersion version, bool removeOtherVersions)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            var oldVersion = await _pluginVersionRepository.GetPluginVersion(pluginId, version.VersionId);
            version.VersionStatus = Status.InReview;
            version.HasAdminConsent = true;

            var notification = new EmailNotification(plugin)
            {
                CallToActionUrl = $"{GetUrlBase()}/Plugins/Edit/{pluginId}/Versions/Pending/{version.VersionId}",
                Message = "A new plugin version was submitted for approval!"
            };

            await Notify(notification, new PushNotification(notification));
            await _loggingRepository.Log(new Log(version, oldVersion, ExtendedUser.AccountName), pluginId);
            return await Save(pluginId, version, "Pending", removeOtherVersions: removeOtherVersions, compareWithManifest: true);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Approve")]
        [RoleAuthorize("SystemAdministrator")]
        public async Task<IActionResult> Approve(int pluginId, PluginVersion version, bool removeOtherVersions = false)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            var oldVersion = await _pluginVersionRepository.GetPluginVersion(pluginId, version.VersionId);
            var notification = new EmailNotification(plugin)
            {
                CallToActionUrl = $"{GetUrlBase()}/Plugins/Edit/{pluginId}/Versions/Edit/{version.VersionId}",
                Message = "A new plugin version was approved!"
            };

            version.VersionStatus = Status.Active;
            version.IsActive = true;
            await _loggingRepository.Log(new Log(version, oldVersion, ExtendedUser.AccountName), pluginId);
            await Notify(notification, new PushNotification(notification));
            return await Save(pluginId, version, "Edit", removeOtherVersions, true);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Reject")]
        [RoleAuthorize("SystemAdministrator")]
        public async Task<IActionResult> Reject(int pluginId, PluginVersion version, bool removeOtherVersions = false)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            var notification = new EmailNotification(plugin)
            {
                CallToActionUrl = $"{GetUrlBase()}/Plugins/Edit/{pluginId}/Versions/Draft/{version.VersionId}",
                Message = "A new plugin version was rejected!"
            };

            version.VersionStatus = Status.Draft;
            version.HasAdminConsent = true;
            await _loggingRepository.Log(new Log
            {
                Author = ExtendedUser.AccountName,
                Description = TemplateResource.RejectedVersionLog,
                TargetInfo = version.VersionNumber
            }, pluginId);

            await Notify(notification, new PushNotification(notification));
            return await Save(pluginId, version, "Draft", removeOtherVersions);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/SaveAsDraft")]
        [RoleAuthorize("Developer", "Administrator")]
        public async Task<IActionResult> SaveAsDraft(int pluginId, PluginVersion version)
        {
            version.VersionStatus = Status.Draft;
            version.HasAdminConsent = false;
            return await Save(pluginId, version, "Draft");
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Save")]
        public async Task<IActionResult> Save(int pluginId, PluginVersion version)
        {
            var oldVersion = await _pluginVersionRepository.GetPluginVersion(pluginId, version.VersionId);
            version.VersionStatus = Status.Active;
            await _loggingRepository.Log(new Log(version, oldVersion, ExtendedUser.AccountName), pluginId);
            return await Save(pluginId, version, "Edit", compareWithManifest: true);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/RequestDeletion/{versionId}")]
        [RoleAuthorize("Developer", "Administrator")]
        public async Task<IActionResult> RequestDeletion(int pluginId, string versionId)
        {
            var version = await _pluginVersionRepository.GetPluginVersion(pluginId, versionId);
            if (!version.IsActive)
            {
                return await Delete(pluginId, versionId);
            }

            var plugin = await _pluginRepository.GetPluginById(pluginId);
            version.NeedsDeletionApproval = true;
            await _pluginVersionRepository.Save(pluginId, version);

            var notification = new EmailNotification(plugin)
            {
                CallToActionUrl = $"{GetUrlBase()}/Plugins/Edit/{pluginId}/Versions",
                Message = "A new plugin version deletion request was sent!"
            };

            await Notify(notification, new PushNotification(notification));
            await _loggingRepository.Log(new Log
            {
                Author = ExtendedUser.AccountName,
                Description = TemplateResource.VersionDeletionRequestLog,
                TargetInfo = version.VersionNumber
            }, pluginId);
            TempData["StatusMessage"] = "Success! Version deletion request was sent!";
            return new EmptyResult();
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/AcceptDeletion/{versionId}")]
        [RoleAuthorize("SystemAdministrator")]
        public async Task<IActionResult> AcceptDeletion(int pluginId, string versionId)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            var version = await _pluginVersionRepository.GetPluginVersion(pluginId, versionId);
            var notification = new EmailNotification(plugin)
            {
                CallToActionUrl = $"{GetUrlBase()}/Plugins/Edit/{pluginId}/Versions",
                Message = "Plugin version deletion request was accepted!"
            };

            await Notify(notification, new PushNotification(notification));
            await _loggingRepository.Log(new Log
            {
                Author = ExtendedUser.AccountName,
                Description = TemplateResource.VersionDeletionAcceptedLog,
                TargetInfo = version.VersionNumber
            }, pluginId);
            return await Delete(pluginId, versionId);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/RejectDeletion/{versionId}")]
        [RoleAuthorize("SystemAdministrator")]
        public async Task<IActionResult> RejectDeletion(int pluginId, string versionId)
        {
            var version = await _pluginVersionRepository.GetPluginVersion(pluginId, versionId);
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            var notification = new EmailNotification(plugin)
            {
                CallToActionUrl = $"{GetUrlBase()}/Plugins/Edit/{pluginId}/Versions",
                Message = "Plugin version deletion request was rejected!"
            };

            version.NeedsDeletionApproval = false;
            await Notify(notification, new PushNotification(notification));
            await _loggingRepository.Log(new Log
            {
                Author = ExtendedUser.AccountName,
                Description = TemplateResource.VersionDeletionRejectedLog,
                TargetInfo = version.VersionNumber
            }, pluginId);
            await _pluginVersionRepository.Save(pluginId, version);
            TempData["StatusMessage"] = "Success! Version deletion request was rejected!";
            return Content(null);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Delete/{versionId}")]
        public async Task<IActionResult> Delete(int pluginId, string versionId)
        {
            var version = await _pluginVersionRepository.GetPluginVersion(pluginId, versionId);
            await _loggingRepository.Log(new Log
            {
                Author = ExtendedUser.AccountName,
                Description = TemplateResource.VersionRemovedLog,
                TargetInfo = version.VersionNumber
            }, pluginId);
            await _pluginVersionRepository.RemovePluginVersion(pluginId, versionId);
            TempData["StatusMessage"] = "Success! Version was removed!";
            return Content(null);
        }

        private bool IsEligibleVersion(ExtendedPluginVersion version)
        {
            if (version.IsThirdParty)
            {
                return ExtendedUser.IsInRoles("Developer", "Administrator") || ExtendedUser.IsInRole("SystemAdministrator") && version.HasAdminConsent;
            }

            return ExtendedUser.IsInRole("SystemAdministrator");
        }

        private async Task Notify(EmailNotification emailNotification, PushNotification pushNotification)
        {
            await _notificationCenter.Broadcast(emailNotification, "Administrator", "Developer");
            await _notificationCenter.Push(pushNotification);
            await _notificationCenter.Broadcast(emailNotification, "SystemAdministrator");
            await _notificationCenter.Push(pushNotification);
        }

        private async Task<IActionResult> Save(int pluginId, PluginVersion version, string route, bool removeOtherVersions = false, bool compareWithManifest = false)
        {
            try
            {
                await _pluginVersionRepository.Save(pluginId, version, removeOtherVersions);

                if (compareWithManifest)
                {
                    await CompareWithManifest(version);
                }

                TempData["StatusMessage"] = "Success! Version was saved!";
                return Content($"/Plugins/Edit/{pluginId}/Versions/{route}/{version.VersionId}");
            }
            catch (Exception e)
            {
                return PartialView("_StatusMessage", string.Format("Error! {0}!", e.Message));
            }
        }

        private async Task CompareWithManifest(PluginVersion version)
        {
            var response = await PluginPackage.DownloadPlugin(version.DownloadUrl);
            var log = response.CreateVersionMatchLog(version, await _productsRepository.GetAllProducts(), out bool isFullMatch);

            TempData["IsVersionMatch"] = log.IsVersionMatch;
            TempData["IsMinVersionMatch"] = log.IsMinVersionMatch;
            TempData["IsMaxVersionMatch"] = log.IsMaxVersionMatch;
            TempData["IsProductMatch"] = log.IsProductMatch;

            if (isFullMatch)
            {
                return;
            }

            TempData["StatusMessage"] = "Warning! Version was saved but there are manifest conflicts!";
        }

        private async Task<IActionResult> Render(int pluginId, string versionId, Status status, Func<int, string, Task<IActionResult>> callback, string view)
        {
            if (!await _pluginVersionRepository.ExistsVersion(pluginId, versionId))
            {
                return NotFound();
            }

            var version = await _pluginVersionRepository.GetPluginVersion(pluginId, versionId, status: status);

            if (version == null)
            {
                return await callback(pluginId, versionId);
            }

            var plugin = await _pluginRepository.GetPluginById(pluginId);
            var versions = await _pluginVersionRepository.GetPluginVersions(pluginId);
            var extendedPlugin = ExtendedPluginDetails.CopyFrom(plugin);
            var extendedVersion = ExtendedPluginVersion.CopyFrom(version);

            extendedPlugin.Parents = await _productsRepository.GetAllParents();
            extendedPlugin.IsEditMode = true;
            extendedPlugin.Versions = versions.Where(v => !v.IsThirdParty || ExtendedUser.IsInRole("Developer") || v.VersionStatus == Status.Active || ExtendedUser.IsInRole("SystemAdministrator") && v.HasAdminConsent).ToList();

            extendedVersion.SupportedProductsListItems = new MultiSelectList(await _productsRepository.GetAllProducts(), nameof(ProductDetails.Id), nameof(ProductDetails.ProductName));
            extendedVersion.PluginId = pluginId;
            extendedVersion.IsThirdParty = extendedPlugin.IsThirdParty;

            return View(view, (extendedPlugin, extendedVersion));
        }
    }
}
