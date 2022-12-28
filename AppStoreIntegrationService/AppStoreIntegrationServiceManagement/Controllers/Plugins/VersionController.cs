using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Plugins
{
    [Area("Plugins")]
    public class VersionController : Controller
    {
        private readonly IPluginRepository _pluginRepository;
        private readonly IProductsRepository _productsRepository;

        public VersionController(IPluginRepository pluginRepository, IProductsRepository productsRepository)
        {
            _pluginRepository = pluginRepository;
            _productsRepository = productsRepository;
        }

        [HttpGet("/Plugins/Edit/{id}/Versions")]
        public async Task<IActionResult> Index(int id)
        {
            var plugin = new ExtendedPluginDetails(await _pluginRepository.GetPluginById(id))
            {
                Parents = await _productsRepository.GetAllParents(),
                IsEditMode = true
            };

            var versions = plugin.Versions?.Select(v => (v.VersionId, v.VersionNumber)).ToList();
            versions.Add((Guid.NewGuid().ToString(), "New version"));
            return View(new KeyValuePair<ExtendedPluginDetails, IEnumerable<(string, string)>>(plugin, versions));
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/{versionId}")]
        public async Task<IActionResult> Show(int pluginId, string versionId)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            var version = plugin.Versions.FirstOrDefault(v => v.VersionId.Equals(versionId));
            var extended = version != null ? new ExtendedPluginVersion(version)
            {
                PluginId = pluginId,
            } : new ExtendedPluginVersion()
            {
                IsNewVersion = true,
                VersionId = versionId,
                PluginId = pluginId
            };
            var products = (await _productsRepository.GetAllProducts()).ToList();
            var parents = (await _productsRepository.GetAllParents()).ToList();
            extended.SetSupportedProductsList(products, parents);
            return PartialView("_PluginVersionDetailsPartial", extended);
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
