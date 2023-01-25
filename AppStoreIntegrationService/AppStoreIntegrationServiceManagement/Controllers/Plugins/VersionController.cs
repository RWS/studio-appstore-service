using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuGet.Protocol.Plugins;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceManagement.Controllers.Plugins
{
    [Area("Plugins")]
    [Authorize]
    public class VersionController : Controller
    {
        private readonly IPluginRepository _pluginRepository;
        private readonly IProductsRepository _productsRepository;
        private readonly ICommentsRepository _commentsRepository;
        private readonly IPluginVersionRepository _pluginVersionRepository;
        private readonly ILoggingRepository _loggingRepository;

        public VersionController
        (
            IPluginRepository pluginRepository,
            IProductsRepository productsRepository,
            ICommentsRepository commentsRepository,
            ILoggingRepository loggingRepository,
            IPluginVersionRepository pluginVersionRepository
        )
        {
            _pluginRepository = pluginRepository;
            _productsRepository = productsRepository;
            _commentsRepository = commentsRepository;
            _loggingRepository = loggingRepository;
            _pluginVersionRepository = pluginVersionRepository;
        }

        [HttpGet("/Plugins/Edit/{pluginId}/Versions")]
        public async Task<IActionResult> Index(int pluginId)
        {
            var versions = new List<ExtendedPluginVersion>();
            var selectedVersion = Request.Query["selectedVersion"];
            var products = await _productsRepository.GetAllProducts();
            var plugin = await _pluginRepository.GetPluginById(pluginId, Status.All, User);
            var allVersions = plugin.Versions.Concat(plugin.Pending).Concat(plugin.Drafts).DistinctBy(v => v.VersionId);
            var productsList = new MultiSelectList(products, nameof(ProductDetails.Id), nameof(ProductDetails.ProductName));
            var extendedPlugin = ExtendedPluginDetails.CopyFrom(plugin);

            extendedPlugin.Parents = await _productsRepository.GetAllParents();
            extendedPlugin.IsEditMode = true;

            foreach (var version in allVersions)
            {
                var extendedVersion = ExtendedPluginVersion.CopyFrom(version);
                extendedVersion.VersionComments = await _commentsRepository.GetComments(pluginId, version.VersionId);
                extendedVersion.SupportedProductsListItems = productsList;
                extendedVersion.PluginId = pluginId;
                extendedVersion.IsThirdParty = extendedPlugin.IsThirdParty;
                versions.Add(extendedVersion);
            }

            versions.Add(new ExtendedPluginVersion
            {
                VersionId = versions.Any(v => v.VersionId == selectedVersion) || string.IsNullOrEmpty(selectedVersion) ? Guid.NewGuid().ToString() : selectedVersion,
                SupportedProductsListItems = productsList,
                IsNewVersion = true,
                PluginId = pluginId,
                VersionStatus = Status.Inactive,
                IsThirdParty = extendedPlugin.IsThirdParty
            });

            return View((extendedPlugin, versions.Where(v => !v.IsThirdParty || User.IsInRole("Developer") || User.IsInRole("StandardUser") && v.VersionStatus == Status.Active || User.IsInRole("Administrator") && v.HasAdminConsent)));
        }

        public async Task<IActionResult> Save(int pluginId, PluginVersion version, object jsonResponse, string log = null, bool removeOtherVersions = false, bool compareWithManifest = false)
        {
            try
            {
                var old = await _pluginVersionRepository.GetPluginVersion(pluginId, version.VersionId, User);

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
                return Json(jsonResponse);
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
            string log = $"<b>{User.Identity.Name} </b> changed the status to <i>Active</i> for the version with number <b>{version.VersionNumber}</b> at {DateTime.Now}";
            return await Save(pluginId, version, new { SelectedView = "Details" }, log);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Deactivate")]
        public async Task<IActionResult> Deactivate(int pluginId, PluginVersion version)
        {
            version.VersionStatus = Status.Inactive;
            string log = $"<b>{User.Identity.Name} </b> changed the status to <i>Inactive</i> for the version with number <b>{version.VersionNumber}</b> at {DateTime.Now}";
            return await Save(pluginId, version, new { SelectedView = "Details" }, log);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Submit")]
        [Authorize(Roles = "Developer")]
        public async Task<IActionResult> Submit(int pluginId, PluginVersion version, bool removeOtherVersions)
        {
            version.VersionStatus = Status.InReview;
            version.HasAdminConsent = true;
            return await Save(pluginId, version, new { SelectedView = "Pending" }, removeOtherVersions: removeOtherVersions, compareWithManifest: true);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Approve")]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> Approve(int pluginId, PluginVersion version, bool removeOtherVersions = false)
        {
            version.VersionStatus = Status.Active;
            version.IsActive = true;
            string log = $"<b>{User.Identity.Name}</b> approved the changes for the version with number <b>{version.VersionNumber}</b> at {DateTime.Now}";
            return await Save(pluginId, version, new { SelectedView = "Details" }, log, removeOtherVersions, true);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Reject")]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> Reject(int pluginId, PluginVersion version, bool removeOtherVersions = false)
        {
            version.VersionStatus = Status.Draft;
            version.HasAdminConsent = true;
            string log = $"<b>{User.Identity.Name}</b> rejected the changes for the version with number <b>{version.VersionNumber}</b> at {DateTime.Now}";
            return await Save(pluginId, version, new { SelectedView = "Draft" }, log, removeOtherVersions);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/SaveAsDraft")]
        [Authorize(Roles = "Developer")]
        public async Task<IActionResult> SaveAsDraft(int pluginId, PluginVersion version)
        {
            version.VersionStatus = Status.Draft;
            version.HasAdminConsent = false;
            return await Save(pluginId, version, new { SelectedView = "Draft" });
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Save")]
        public async Task<IActionResult> Save(int pluginId, PluginVersion version)
        {
            return await Save(pluginId, version, new { SelectedView = "Details" }, compareWithManifest: true);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/RequestDeletion/{versionId}")]
        [Authorize(Roles = "Developer")]
        public async Task<IActionResult> RequestDeletion(int pluginId, string versionId)
        {
            var version = await _pluginVersionRepository.GetPluginVersion(pluginId, versionId, User);
            if (version.IsActive)
            {
                version.NeedsDeletionApproval = true;
                await _pluginVersionRepository.Save(pluginId, version);
                await _loggingRepository.Log(User.Identity.Name, pluginId, $"<b>{User.Identity.Name}</b> sent a deletion request for version with number <b>{version.VersionNumber}</b> at {DateTime.Now}");
                TempData["StatusMessage"] = "Success! Version deletion request was sent!";
                return Content(null);
            }

            return await Delete(pluginId, versionId);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/AcceptDeletion/{versionId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AcceptDeletion(int pluginId, string versionId)
        {
            var version = await _pluginVersionRepository.GetPluginVersion(pluginId, versionId, User);
            await _loggingRepository.Log(User.Identity.Name, pluginId, $"<b>{User.Identity.Name}</b> accepted the deletion request for version with number <b>{version.VersionNumber}</b> at {DateTime.Now}");
            return await Delete(pluginId, versionId);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/RejectDeletion/{versionId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RejectDeletion(int pluginId, string versionId)
        {
            var version = await _pluginVersionRepository.GetPluginVersion(pluginId, versionId, User);
            version.NeedsDeletionApproval = false;
            await _loggingRepository.Log(User.Identity.Name, pluginId, $"<b>{User.Identity.Name}</b> rejected the deletion request for version with number <b>{version.VersionNumber}</b> at {DateTime.Now}");
            await _pluginVersionRepository.Save(pluginId, version);
            TempData["StatusMessage"] = "Success! Version deletion request was rejected!";
            return Content(null);
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Delete/{versionId}")]
        public async Task<IActionResult> Delete(int pluginId, string versionId)
        {
            var version = await _pluginVersionRepository.GetPluginVersion(pluginId, versionId, User);
            await _loggingRepository.Log(User.Identity.Name, pluginId, $"<b>{User.Identity.Name}</b> removed the version with number <b>{version.VersionNumber}</b> at {DateTime.Now}");
            await _pluginVersionRepository.RemovePluginVersion(pluginId, versionId);
            TempData["StatusMessage"] = "Success! Version was removed!";
            return Json(new { SelectedView = string.Empty, SelectedVersion = string.Empty });
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
