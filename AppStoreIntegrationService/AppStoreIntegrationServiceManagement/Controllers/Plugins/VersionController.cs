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
                Parents = await _productsRepository.GetAllParents()
            };

            var versions = plugin.Versions?.Select(v => (v.VersionId, v.VersionNumber)).ToList();
            versions.Add((Guid.NewGuid().ToString(), "New version"));
            return View(new KeyValuePair<ExtendedPluginDetails, IEnumerable<(string, string)>>(plugin, versions));
        }

        [HttpPost("/Plugins/Edit/{pluginId}/Versions/{versionId}")]
        public async Task<IActionResult> Show(int pluginId, string versionId)
        {
            var plugin = await _pluginRepository.GetPluginById(pluginId);
            var version = new ExtendedPluginVersion(plugin.Versions.FirstOrDefault(v => v.VersionId.Equals(versionId)));
            var products = (await _productsRepository.GetAllProducts()).ToList();
            var parents = (await _productsRepository.GetAllParents()).ToList();
            version.SetSupportedProductsList(products, parents);
            return PartialView("_PluginVersionDetailsPartial", version);
        }

        [HttpPost]
        public async Task<IActionResult> Add()
        {
            var products = (await _productsRepository.GetAllProducts()).ToList();
            var parents = (await _productsRepository.GetAllParents()).ToList();
            var version = new ExtendedPluginVersion
            {
                VersionName = "New plugin version",
                VersionNumber = string.Empty,
                IsPrivatePlugin = true,
                IsNewVersion = true,
                VersionId = Guid.NewGuid().ToString()
            };

            version.SetSupportedProductsList(products, parents);

            return PartialView("_PluginVersionDetailsPartial", version);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateChecksum(ExtendedPluginVersion version)
        {
            try
            {
                var remoteReader = new RemoteStreamReader(new Uri(version.VersionDownloadUrl));
                version.FileHash = SHA1Generator.GetHash(await remoteReader.ReadAsStreamAsync());
            }
            catch (Exception e)
            {
                return PartialView("_StatusMessage", $"Error! {e.Message}");
            }

            TempData["Filehash"] = version.FileHash;
            return PartialView("_StatusMessage", "Success! Checksum was computed!");
        }

        [HttpPost]
        [Route("[controller]/[action]/{versionId}")]
        public async Task<IActionResult> Delete(ExtendedPluginDetails plugin, string versionId)
        {
            await _pluginRepository.RemovePluginVersion(plugin.Id, versionId);
            TempData["StatusMessage"] = "Success! Version was removed!";
            return Content($"Plugins/Edit/{plugin.Id}");
        }
    }
}
