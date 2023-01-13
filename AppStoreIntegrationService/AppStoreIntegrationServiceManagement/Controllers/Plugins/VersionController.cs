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
        private readonly ILoggingRepository _loggingRepository;

        public VersionController
        (
            IPluginRepository pluginRepository,
            IProductsRepository productsRepository,
            ICommentsRepository commentsRepository,
            ILoggingRepository loggingRepository
        )
        {
            _pluginRepository = pluginRepository;
            _productsRepository = productsRepository;
            _commentsRepository = commentsRepository;
            _loggingRepository = loggingRepository;
        }

        [HttpGet("/Plugins/Edit/{pluginId}/Versions")]
        public async Task<IActionResult> Index(int pluginId)
        {
            var versions = new List<ExtendedPluginVersion>();
            var selectedVersion = Request.Query["selectedVersion"];
            var parents = await _productsRepository.GetAllParents();
            var products = await _productsRepository.GetAllProducts();
            var plugin = await _pluginRepository.GetPluginById(pluginId, User);
            var extendedPlugin = new ExtendedPluginDetails(plugin) { Parents = parents, IsEditMode = true };
            var productsList = new MultiSelectList(products, nameof(ProductDetails.Id), nameof(ProductDetails.ProductName));

            foreach (var version in extendedPlugin.Versions)
            {
                versions.Add(new ExtendedPluginVersion(version)
                {
                    VersionComments = await _commentsRepository.GetComments(pluginId, version.VersionId),
                    SupportedProductsListItems = productsList,
                    PluginId = pluginId,
                    IsThirdParty = extendedPlugin.IsThirdParty
                });
            }

            versions.Add(new ExtendedPluginVersion
            {
                VersionId = versions.Any(v => v.VersionId == selectedVersion) || string.IsNullOrEmpty(selectedVersion) ? Guid.NewGuid().ToString() : selectedVersion,
                SupportedProductsListItems = productsList,
                IsNewVersion = true,
                PluginId = pluginId,
                IsThirdParty = extendedPlugin.IsThirdParty
            });

            return View((extendedPlugin, versions));
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Save")]
        public async Task<IActionResult> Save(PluginVersion version, bool isNewVersion, int pluginId)
        {
            try
            {
                version.HasAdminConsent = version.HasAdminConsent || version.VersionStatus.Equals(Status.InReview);
                var old = await _pluginRepository.GetPluginVersion(pluginId, version.VersionId, User);
                await _loggingRepository.Log(User, pluginId, new PluginVersionBase<string>(version), old == null ? old : new PluginVersionBase<string>(old));
                await _pluginRepository.UpdatePluginVersion(pluginId, version);
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
            catch (Exception e)
            {
                TempData["StatusMessage"] = $"Error! {e.Message}.";
                return new EmptyResult();
            }

            TempData["StatusMessage"] = string.Format("Success! Version was {0}!", isNewVersion ? "added" : "updated");
            return new EmptyResult();
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

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Delete/{versionId}")]
        public async Task<IActionResult> Delete(int pluginId, string versionId, bool deletionApproval)
        {
            var version = await _pluginRepository.GetPluginVersion(pluginId, versionId, User);

            if (User.IsInRole("Developer") && version.HasAdminConsent || (User.IsInRole("Administrator") && !deletionApproval))
            {
                version.NeedsDeletionApproval = deletionApproval;
                await _pluginRepository.UpdatePluginVersion(pluginId, version);
                TempData["StatusMessage"] = string.Format("Success! Version deletion request was {0}!", deletionApproval ? "sent" : "rejected");
                return Content(null);
            }

            await _loggingRepository.Log(User, pluginId, $"<b>{User.Identity.Name}</b> removed the version with number <b>{version.VersionNumber}</b> at {DateTime.Now}");
            await _pluginRepository.RemovePluginVersion(pluginId, versionId);
            TempData["StatusMessage"] = "Success! Version was removed!";
            return Content(null);
        }
    }
}
