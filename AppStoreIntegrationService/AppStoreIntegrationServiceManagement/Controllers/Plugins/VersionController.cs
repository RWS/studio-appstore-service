using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppStoreIntegrationServiceManagement.Controllers.Plugins
{
    [Area("Plugins")]
    public class VersionController : Controller
    {
        private readonly IPluginRepository _pluginRepository;
        private readonly IProductsRepository _productsRepository;
        private readonly ICommentsRepository _commentsRepository;

        public VersionController(IPluginRepository pluginRepository, IProductsRepository productsRepository, ICommentsRepository commentsRepository)
        {
            _pluginRepository = pluginRepository;
            _productsRepository = productsRepository;
            _commentsRepository = commentsRepository;
        }

        [HttpGet("/Plugins/Edit/{id}/Versions")]
        public async Task<IActionResult> Index(int id)
        {
            var selectedVersion = Request.Query["selectedVersion"];
            var plugin = new ExtendedPluginDetails(await _pluginRepository.GetPluginById(id))
            {
                Parents = await _productsRepository.GetAllParents(),
                IsEditMode = true
            };

            var products = new MultiSelectList(await _productsRepository.GetAllProducts(), nameof(ProductDetails.Id), nameof(ProductDetails.ProductName));
            var versions = (await Task.WhenAll(plugin.Versions?.Select(async v => new ExtendedPluginVersion(v) 
            { 
                VersionComments = await _commentsRepository.GetCommentsForVersion(plugin.Name, v.VersionId),
                SupportedProductsListItems = products
            }))).ToList();

            versions.Add(new ExtendedPluginVersion 
            { 
                VersionId = versions.Any(v => v.VersionId == selectedVersion) ? Guid.NewGuid().ToString() : selectedVersion,
                SupportedProductsListItems = products,
                IsNewVersion = true
            });
            return View((plugin, versions));
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/Save")]
        public async Task<IActionResult> Save(ExtendedPluginVersion version, int pluginId)
        {
            try
            {
                await _pluginRepository.UpdatePluginVersion(pluginId, version);
                var response = await PluginPackage.DownloadPlugin(version.VersionDownloadUrl);
                TempData["ManifestVersionCompare"] = response.CreateVersionMatchLog(version, await _productsRepository.GetAllProducts(), out bool isFullMatch);

                if (!isFullMatch)
                {
                    return PartialView("_StatusMessage", "Warning! Version was saved but there are manifest conflicts!");
                }
            }
            catch (Exception e)
            {
                TempData["StatusMessage"] = $"Error! {e.Message}.";
            }

            TempData["StatusMessage"] = string.Format("Success! Version was {0}!", version.IsNewVersion ? "added" : "updated");
            return Content(null);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateChecksum(string versionDownloadUrl)
        {
            try
            {
                var remoteReader = new RemoteStreamReader(new Uri(versionDownloadUrl));
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
        public async Task<IActionResult> Delete(int pluginId, string versionId)
        {
            await _pluginRepository.RemovePluginVersion(pluginId, versionId);
            TempData["StatusMessage"] = "Success! Version was removed!";
            return Content(null);
        }
    }
}
