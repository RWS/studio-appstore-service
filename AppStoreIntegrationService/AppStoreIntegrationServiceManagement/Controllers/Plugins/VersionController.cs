using AppStoreIntegrationServiceCore.Model;
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
    [AccountSelected]
    public class VersionController : CustomController
    {
        private readonly IPluginRepository _pluginRepository;
        private readonly IProductsRepository _productsRepository;
        private readonly IPluginVersionRepository _pluginVersionRepository;
        private readonly ILoggingRepository _loggingRepository;
        private readonly INotificationCenter _notificationCenter;

        public VersionController
        (
            IPluginRepository pluginRepository,
            IProductsRepository productsRepository,
            ILoggingRepository loggingRepository,
            IPluginVersionRepository pluginVersionRepository,
            INotificationCenter notificationCenter
        )
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

            return View((extendedPlugin, extendedVersions.Where(v => v.IsThirdParty && ExtendedUser.IsInRole("Developer") || ExtendedUser.HasFullOwnership() && v.HasAdminConsent)));
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
            extendedPlugin.Versions = versions.Where(v => v.IsThirdParty && ExtendedUser.IsInRole("Developer") || ExtendedUser.HasFullOwnership() && v.HasAdminConsent).ToList();

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
        public async Task<IActionResult> Activate(int pluginId, PluginVersion version)
        {
            version.VersionStatus = Status.Active;
            string log = string.Format(TemplateResource.VersionActiveLog, User.Identity.Name, version.VersionNumber, DateTime.Now);
            return await Save(pluginId, version, "Edit", log);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Deactivate")]
        public async Task<IActionResult> Deactivate(int pluginId, PluginVersion version)
        {
            version.VersionStatus = Status.Inactive;
            string log = string.Format(TemplateResource.VersionInactiveLog, User.Identity.Name, version.VersionNumber, DateTime.Now);
            return await Save(pluginId, version, "Edit", log);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Submit")]
        [RoleAuthorize("Developer")]
        public async Task<IActionResult> Submit(int pluginId, PluginVersion version, bool removeOtherVersions)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            version.VersionStatus = Status.InReview;
            version.HasAdminConsent = true;

            var notification = new EmailNotification(plugin)
            {
                CallToActionUrl = $"{GetUrlBase()}/Plugins/Edit/{pluginId}/Versions/Pending/{version.VersionId}",
                Message = "A new plugin version was submitted for approval!"
            };

            await Notify(notification, new PushNotification(notification));
            return await Save(pluginId, version, "Pending", removeOtherVersions: removeOtherVersions, compareWithManifest: true);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Approve")]
        [RoleAuthorize("Administrator")]
        [Owner]
        public async Task<IActionResult> Approve(int pluginId, PluginVersion version, bool removeOtherVersions = false)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            string log = string.Format(TemplateResource.ApprovedVersionLog, User.Identity.Name, version.VersionNumber, DateTime.Now);
            version.VersionStatus = Status.Active;
            version.IsActive = true;

            var notification = new EmailNotification(plugin)
            {
                CallToActionUrl = $"{GetUrlBase()}/Plugins/Edit/{pluginId}/Versions/Edit/{version.VersionId}",
                Message = "A new plugin version was approved!"
            };

            await Notify(notification, new PushNotification(notification));
            return await Save(pluginId, version, "Edit", log, removeOtherVersions, true);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Reject")]
        [RoleAuthorize("Administrator")]
        [Owner]
        public async Task<IActionResult> Reject(int pluginId, PluginVersion version, bool removeOtherVersions = false)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            var log = string.Format(TemplateResource.RejectedVersionLog, User.Identity.Name, version.VersionNumber, DateTime.Now);
            version.VersionStatus = Status.Draft;
            version.HasAdminConsent = true;

            var notification = new EmailNotification(plugin)
            {
                CallToActionUrl = $"{GetUrlBase()}/Plugins/Edit/{pluginId}/Versions/Draft/{version.VersionId}",
                Message = "A new plugin version was rejected!"
            };

            await Notify(notification, new PushNotification(notification));
            return await Save(pluginId, version, "Draft", log, removeOtherVersions);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/SaveAsDraft")]
        [RoleAuthorize("Developer")]
        public async Task<IActionResult> SaveAsDraft(int pluginId, PluginVersion version)
        {
            version.VersionStatus = Status.Draft;
            version.HasAdminConsent = false;
            return await Save(pluginId, version, "Draft");
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Save")]
        public async Task<IActionResult> Save(int pluginId, PluginVersion version)
        {
            version.VersionStatus = Status.Active;
            return await Save(pluginId, version, "Edit", compareWithManifest: true);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/RequestDeletion/{versionId}")]
        [RoleAuthorize("Developer")]
        public async Task<IActionResult> RequestDeletion(int pluginId, string versionId)
        {
            var version = await _pluginVersionRepository.GetPluginVersion(pluginId, versionId);
            if (!version.IsActive)
            {
                return await Delete(pluginId, versionId);
            }

            var plugin = await _pluginRepository.GetPluginById(pluginId);
            var log = string.Format(TemplateResource.VersionDeletionRequestLog, User.Identity.Name, version.VersionNumber, DateTime.Now);
            version.NeedsDeletionApproval = true;
            await _pluginVersionRepository.Save(pluginId, version);

            var notification = new EmailNotification(plugin)
            {
                CallToActionUrl = $"{GetUrlBase()}/Plugins/Edit/{pluginId}/Versions",
                Message = "A new plugin version deletion request was sent!"
            };

            await Notify(notification, new PushNotification(notification));
            await _loggingRepository.Log(User.Identity.Name, pluginId, log);
            TempData["StatusMessage"] = "Success! Version deletion request was sent!";
            return new EmptyResult();
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/AcceptDeletion/{versionId}")]
        [RoleAuthorize("Administrator")]
        [Owner]
        public async Task<IActionResult> AcceptDeletion(int pluginId, string versionId)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            var version = await _pluginVersionRepository.GetPluginVersion(pluginId, versionId);
            var log = string.Format(TemplateResource.VersionDeletionAcceptedLog, User.Identity.Name, version.VersionNumber, DateTime.Now);
            
            var notification = new EmailNotification(plugin)
            {
                CallToActionUrl = $"{GetUrlBase()}/Plugins/Edit/{pluginId}/Versions",
                Message = "Plugin version deletion request was accepted!"
            };

            await Notify(notification, new PushNotification(notification));
            await _loggingRepository.Log(User.Identity.Name, pluginId, log);
            return await Delete(pluginId, versionId);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/RejectDeletion/{versionId}")]
        [RoleAuthorize("Administrator")]
        [Owner]
        public async Task<IActionResult> RejectDeletion(int pluginId, string versionId)
        {
            var version = await _pluginVersionRepository.GetPluginVersion(pluginId, versionId);
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            var log = string.Format(TemplateResource.VersionDeletionRejectedLog, User.Identity.Name, version.VersionNumber, DateTime.Now);
            version.NeedsDeletionApproval = false;

            var notification = new EmailNotification(plugin)
            {
                CallToActionUrl = $"{GetUrlBase()}/Plugins/Edit/{pluginId}/Versions",
                Message = "Plugin version deletion request was rejected!"
            };

            await Notify(notification, new PushNotification(notification));
            await _loggingRepository.Log(User.Identity.Name, pluginId, log);
            await _pluginVersionRepository.Save(pluginId, version);
            TempData["StatusMessage"] = "Success! Version deletion request was rejected!";
            return Content(null);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Delete/{versionId}")]
        public async Task<IActionResult> Delete(int pluginId, string versionId)
        {
            var version = await _pluginVersionRepository.GetPluginVersion(pluginId, versionId);
            var log = string.Format(TemplateResource.VersionRemovedLog, User.Identity.Name, version.VersionNumber, DateTime.Now);
            await _loggingRepository.Log(User.Identity.Name, pluginId, log);
            await _pluginVersionRepository.RemovePluginVersion(pluginId, versionId);
            TempData["StatusMessage"] = "Success! Version was removed!";
            return Content(null);
        }

        private async Task Notify(EmailNotification emailNotification, PushNotification pushNotification)
        {
            await _notificationCenter.SendEmail(emailNotification);
            await _notificationCenter.Push(pushNotification);
            await _notificationCenter.Broadcast(emailNotification);
            pushNotification.Author = AccountsManager.GetAppStoreAccount().AccountName;
            await _notificationCenter.Push(pushNotification);
        }

        private async Task<IActionResult> Save(int pluginId, PluginVersion version, string route, string log = null, bool removeOtherVersions = false, bool compareWithManifest = false)
        {
            try
            {
                var old = await _pluginVersionRepository.GetPluginVersion(pluginId, version.VersionId);

                if (string.IsNullOrEmpty(log))
                {
                    log = _loggingRepository.CreateChangesLog(version, old, User.Identity.Name);
                    await _loggingRepository.Log(User.Identity.Name, pluginId, log);
                }
                else
                {
                    await _loggingRepository.Log(User.Identity.Name, pluginId, log);
                }

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
            extendedPlugin.Versions = versions.Where(v => !v.IsThirdParty || ExtendedUser.IsInRole("Developer") || v.VersionStatus == Status.Active || ExtendedUser.HasFullOwnership() && v.HasAdminConsent).ToList();

            extendedVersion.SupportedProductsListItems = new MultiSelectList(await _productsRepository.GetAllProducts(), nameof(ProductDetails.Id), nameof(ProductDetails.ProductName));
            extendedVersion.PluginId = pluginId;
            extendedVersion.IsThirdParty = extendedPlugin.IsThirdParty;

            return View(view, (extendedPlugin, extendedVersion));
        }
    }
}
