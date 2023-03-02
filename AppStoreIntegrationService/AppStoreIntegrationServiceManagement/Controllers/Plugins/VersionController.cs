using AppStoreIntegrationServiceCore;
using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceManagement.Helpers;
using AppStoreIntegrationServiceManagement.Model.DataBase;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using AppStoreIntegrationServiceManagement.Model.Repository;
using AppStoreIntegrationServiceManagement.Model.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceManagement.Controllers.Plugins
{
    [Area("Plugins")]
    [Authorize]
    public class VersionController : Controller
    {
        private readonly IPluginRepository _pluginRepository;
        private readonly IProductsRepository _productsRepository;
        private readonly IPluginVersionRepository _pluginVersionRepository;
        private readonly ILoggingRepository _loggingRepository;
        private readonly INotificationCenter _notificationCenter;
        private readonly UserManager<IdentityUserExtended> _userManager;

        public VersionController
        (
            IPluginRepository pluginRepository,
            IProductsRepository productsRepository,
            ILoggingRepository loggingRepository,
            IPluginVersionRepository pluginVersionRepository,
            UserManager<IdentityUserExtended> userManager,
            INotificationCenter notificationCenter
        )
        {
            _pluginRepository = pluginRepository;
            _productsRepository = productsRepository;
            _loggingRepository = loggingRepository;
            _pluginVersionRepository = pluginVersionRepository;
            _notificationCenter = notificationCenter;
            _userManager = userManager;
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

            return View((extendedPlugin, extendedVersions.Where(v => !v.IsThirdParty || User.IsInRole("Developer") || User.IsInRole("StandardUser") && v.VersionStatus == Status.Active || User.IsInRole("Administrator") && v.HasAdminConsent)));
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
            extendedPlugin.Versions = versions.Where(v => !v.IsThirdParty || User.IsInRole("Developer") || User.IsInRole("StandardUser") && v.VersionStatus == Status.Active || User.IsInRole("Administrator") && v.HasAdminConsent).ToList();

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
        [Authorize(Roles = "Developer")]
        public async Task<IActionResult> Submit(int pluginId, PluginVersion version, bool removeOtherVersions)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            version.VersionStatus = Status.InReview;
            version.HasAdminConsent = true;
            await Notify(NotificationTemplate.VersionReviewRequest, plugin, version.VersionId);
            return await Save(pluginId, version, "Pending", removeOtherVersions: removeOtherVersions, compareWithManifest: true);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Approve")]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> Approve(int pluginId, PluginVersion version, bool removeOtherVersions = false)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            string log = string.Format(TemplateResource.ApprovedVersionLog, User.Identity.Name, version.VersionNumber, DateTime.Now);
            version.VersionStatus = Status.Active;
            version.IsActive = true;
            await Notify(NotificationTemplate.VersionApprovedRequest, plugin, version.VersionId);
            return await Save(pluginId, version, "Edit", log, removeOtherVersions, true);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Reject")]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> Reject(int pluginId, PluginVersion version, bool removeOtherVersions = false)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            var log = string.Format(TemplateResource.RejectedVersionLog, User.Identity.Name, version.VersionNumber, DateTime.Now);
            version.VersionStatus = Status.Draft;
            version.HasAdminConsent = true;
            await Notify(NotificationTemplate.VersionRejectedRequest, plugin, version.VersionId);
            return await Save(pluginId, version, "Draft", log, removeOtherVersions);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/SaveAsDraft")]
        [Authorize(Roles = "Developer")]
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
        [Authorize(Roles = "Developer")]
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
            await Notify(NotificationTemplate.VersionDeletionRequest, plugin, version.VersionId);
            await _loggingRepository.Log(User.Identity.Name, pluginId, log);
            TempData["StatusMessage"] = "Success! Version deletion request was sent!";
            return new EmptyResult();
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/AcceptDeletion/{versionId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AcceptDeletion(int pluginId, string versionId)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            var version = await _pluginVersionRepository.GetPluginVersion(pluginId, versionId);
            var log = string.Format(TemplateResource.VersionDeletionAcceptedLog, User.Identity.Name, version.VersionNumber, DateTime.Now);
            await Notify(NotificationTemplate.VersionDeletionApproved, plugin, version.VersionId);
            await _loggingRepository.Log(User.Identity.Name, pluginId, log);
            return await Delete(pluginId, versionId);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/RejectDeletion/{versionId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RejectDeletion(int pluginId, string versionId)
        {
            var version = await _pluginVersionRepository.GetPluginVersion(pluginId, versionId);
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            var log = string.Format(TemplateResource.VersionDeletionRejectedLog, User.Identity.Name, version.VersionNumber, DateTime.Now);
            version.NeedsDeletionApproval = false;
            await Notify(NotificationTemplate.VersionDeletionRejected, plugin, version.VersionId);
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

        private async Task Notify(NotificationTemplate notificationTemplate, PluginDetails plugin, string versionId)
        {
            var emailNotification = _notificationCenter.GetNotification(notificationTemplate, true, plugin.Icon.MediaUrl, plugin.Name, plugin.Id, versionId);
            var pushNotification = _notificationCenter.GetNotification(notificationTemplate, false, plugin.Icon.MediaUrl, plugin.Name, plugin.Id, versionId);
            await _notificationCenter.SendEmail(emailNotification, await GetCurrentPluginUserEmail(plugin.Developer.DeveloperName));
            await _notificationCenter.Push(pushNotification, plugin.Developer.DeveloperName);
            await _notificationCenter.Broadcast(emailNotification);
            await _notificationCenter.Push(pushNotification);
        }

        private async Task<string> GetCurrentPluginUserEmail(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            return user.Email;
        }

        private async Task<IActionResult> Save(int pluginId, PluginVersion version, string route, string log = null, bool removeOtherVersions = false, bool compareWithManifest = false)
        {
            try
            {
                var old = await _pluginVersionRepository.GetPluginVersion(pluginId, version.VersionId);

                if (string.IsNullOrEmpty(log))
                {
                    await _loggingRepository.Log(User.Identity.Name, pluginId, CreateChangesLog(version, old));
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
            extendedPlugin.Versions = versions.Where(v => !v.IsThirdParty || User.IsInRole("Developer") || User.IsInRole("StandardUser") && v.VersionStatus == Status.Active || User.IsInRole("Administrator") && v.HasAdminConsent).ToList();

            extendedVersion.SupportedProductsListItems = new MultiSelectList(await _productsRepository.GetAllProducts(), nameof(ProductDetails.Id), nameof(ProductDetails.ProductName));
            extendedVersion.PluginId = pluginId;
            extendedVersion.IsThirdParty = extendedPlugin.IsThirdParty;

            return View(view, (extendedPlugin, extendedVersion));
        }

        private string CreateChangesLog(PluginVersion @new, PluginVersion old)
        {
            var oldToBase = PluginVersionBase<string>.CopyFrom(old);
            var newToBase = PluginVersionBase<string>.CopyFrom(@new);

            if (oldToBase == null)
            {
                return $"<b>{User.Identity.Name}</b> added version with number {@new.VersionNumber} at {DateTime.Now}<br><br><p>The version properties are:</p><ul>{CreateNewLog(newToBase)}</ul>";
            }

            if (oldToBase.Equals(newToBase))
            {
                return null;
            }

            return $"<b>{User.Identity.Name}</b> made changes to the version with number {@new.VersionNumber} at {DateTime.Now}<br><br><p>The following changes occured:</p><ul>{CreateComparisonLog(newToBase, oldToBase)}</ul>";
        }

        private string CreateNewLog(PluginVersionBase<string> version)
        {
            string change = "<li><b>{0}</b> : <i>{1}</i></li>";
            return string.Format(change, "File hash", version.FileHash) +
                   string.Format(change, "Download URL", version.DownloadUrl) +
                   string.Format(change, "Status", version.VersionStatus.ToString()) +
                   string.Format(change, "Is private plugin", version.IsPrivatePlugin) +
                   string.Format(change, "Is navigation link", version.IsNavigationLink) +
                   string.Format(change, "Supported products", CreateProductsLog(version.SupportedProducts)) +
                   string.Format(change, "Plugin has studio installer", version.AppHasStudioPluginInstaller) +
                   string.Format(change, "Minimum required studio version", version.MinimumRequiredVersionOfStudio) +
                   string.Format(change, "Maximum required studio version", version.MaximumRequiredVersionOfStudio);
        }

        private string CreateComparisonLog(PluginVersionBase<string> @new, PluginVersionBase<string> old)
        {
            string change = "<li>The property <b>{0}</b> changed from <i>{1}</i> to <i>{2}</i></li>";
            return (@new.FileHash == old.FileHash ? null : string.Format(change, "File hash", @new.FileHash, old.FileHash)) +
                   (@new.DownloadUrl == old.DownloadUrl ? null : string.Format(change, "Download URL", @new.DownloadUrl, old.DownloadUrl)) +
                   (@new.VersionStatus == old.VersionStatus ? null : string.Format(change, "Status", @new.VersionStatus, old.VersionStatus)) +
                   (@new.IsPrivatePlugin == old.IsPrivatePlugin ? null : string.Format(change, "Is private plugin", @new.IsPrivatePlugin, old.IsPrivatePlugin)) +
                   (@new.IsNavigationLink == old.IsNavigationLink ? null : string.Format(change, "Is navigation link", @new.IsNavigationLink, old.IsNavigationLink)) +
                   (@new.AppHasStudioPluginInstaller == old.AppHasStudioPluginInstaller ? null : string.Format(change, "Plugin has studio installer", @new.AppHasStudioPluginInstaller, old.AppHasStudioPluginInstaller)) +
                   (@new.SupportedProducts.SequenceEqual(old.SupportedProducts) ? null : string.Format(change, "Supported products", CreateProductsLog(@new.SupportedProducts), CreateProductsLog(@old.SupportedProducts))) +
                   (@new.MinimumRequiredVersionOfStudio == old.MinimumRequiredVersionOfStudio ? null : string.Format(change, "Minimum required studio version", @new.MinimumRequiredVersionOfStudio, old.MinimumRequiredVersionOfStudio)) +
                   (@new.MaximumRequiredVersionOfStudio == old.MaximumRequiredVersionOfStudio ? null : string.Format(change, "Maximum required studio versionr", @new.MaximumRequiredVersionOfStudio, old.MaximumRequiredVersionOfStudio));
        }

        private string CreateProductsLog(List<string> products)
        {
            var productDetails = _productsRepository.GetAllProducts().Result;
            if (products.Count > 1)
            {
                return $"[{products.Aggregate("", (result, next) => $"{result}, {productDetails.FirstOrDefault(c => c.Id == next).ProductName}")}]";
            }

            return $"[{productDetails.FirstOrDefault(c => c.Id == products[0]).ProductName}]";
        }
    }
}
