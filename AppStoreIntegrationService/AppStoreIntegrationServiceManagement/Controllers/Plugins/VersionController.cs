using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using Microsoft.AspNetCore.Authorization;
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
            var productsList = new MultiSelectList(products, nameof(ProductDetails.Id), nameof(ProductDetails.ProductName));
            var extendedPlugin = ExtendedPluginDetails.CopyFrom(plugin);

            extendedPlugin.Parents = await _productsRepository.GetAllParents();
            extendedPlugin.IsEditMode = true;

            foreach (var version in extendedPlugin.Versions)
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

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Save")]
        public async Task<IActionResult> Save(PluginVersion version, bool isNewVersion, int pluginId, bool saveStatusOnly)
        {
            try
            {
                version.HasAdminConsent = version.HasAdminConsent || version.VersionStatus.Equals(Status.InReview);
                version.WasActive = version.WasActive || version.VersionStatus.Equals(Status.Active);
                var old = await _pluginVersionRepository.GetPluginVersion(pluginId, version.VersionId, User);
                await _loggingRepository.Log(User, pluginId, PluginVersionBase<string>.CopyFrom(version), PluginVersionBase<string>.CopyFrom(old));
                await _pluginVersionRepository.UpdatePluginVersion(pluginId, version);

                if (!saveStatusOnly)
                {
                    var response = await PluginPackage.DownloadPlugin(version.DownloadUrl);
                    var log = response.CreateVersionMatchLog(version, await _productsRepository.GetAllProducts(), out bool isFullMatch);

                    TempData["IsVersionMatch"] = log.IsVersionMatch;
                    TempData["IsMinVersionMatch"] = log.IsMinVersionMatch;
                    TempData["IsMaxVersionMatch"] = log.IsMaxVersionMatch;
                    TempData["IsProductMatch"] = log.IsProductMatch;

                    if (!isFullMatch)
                    {
                        TempData["StatusMessage"] = "Warning! Version was saved but there are manifest conflicts!";
                        return new EmptyResult();
                    }
                }

                TempData["StatusMessage"] = string.Format("Success! Version was {0}!", isNewVersion ? "added" : "updated");
                return new EmptyResult();
            }
            catch (Exception e)
            {
                return PartialView("_StatusMessage", string.Format("Error! {0}!", e.Message));
            }
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

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(PluginVersion version, Status status)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Authorize(Roles = "Developer")]
        public async Task<IActionResult> Submit(PluginVersion version, bool removeOtherVersions)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> Approve(PluginVersion version, bool removeOtherVersions = false)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> Reject(PluginVersion version, bool removeOtherVersions = false)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Authorize(Policy = "Developer")]
        public async Task<IActionResult> SaveAsDraft(PluginVersion version)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<IActionResult> Save(PluginVersion version)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Authorize(Roles = "Developer")]
        public async Task<IActionResult> RequestDeletion(int pluginId, string versionId)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RejectDeletion(int pluginId, string versionId)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int pluginId, string versionId)
        {
            throw new NotImplementedException();
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Delete/{versionId}")]
        public async Task<IActionResult> Delete(int pluginId, string versionId, bool deletionApproval)
        {
            var version = await _pluginVersionRepository.GetPluginVersion(pluginId, versionId, User);

            if (User.IsInRole("Developer") && version.HasAdminConsent && version.WasActive || 
               (User.IsInRole("Administrator") && !deletionApproval))
            {
                version.NeedsDeletionApproval = deletionApproval;
                await _pluginVersionRepository.UpdatePluginVersion(pluginId, version);
                TempData["StatusMessage"] = string.Format("Success! Version deletion request was {0}!", deletionApproval ? "sent" : "rejected");
                return Content(null);
            }

            await _loggingRepository.Log(User, pluginId, $"<b>{User.Identity.Name}</b> removed the version with number <b>{version.VersionNumber}</b> at {DateTime.Now}");
            await _pluginVersionRepository.RemovePluginVersion(pluginId, versionId);
            TempData["StatusMessage"] = "Success! Version was removed!";
            return Content(null);
        }
    }
}
