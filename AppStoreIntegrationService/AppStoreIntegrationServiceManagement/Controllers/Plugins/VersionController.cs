using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Plugins
{
    [Area("Plugins")]
    public class VersionController : Controller
    {
        private readonly IPluginRepository<PluginDetails<PluginVersion<string>, string>> _pluginRepository;
        private readonly IProductsRepository _productsRepository;

        public VersionController(IPluginRepository<PluginDetails<PluginVersion<string>, string>> pluginRepository, IProductsRepository productsRepository)
        {
            _pluginRepository = pluginRepository;
            _productsRepository = productsRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Show(List<ExtendedPluginVersion<string>> versions, PrivatePlugin<PluginVersion<string>> plugin)
        {
            var version = versions.FirstOrDefault(v => v.VersionId.Equals(plugin.SelectedVersionId));
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
            var version = new ExtendedPluginVersion<string>
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
        public async Task<IActionResult> GenerateChecksum(ExtendedPluginVersion<string> version)
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
        public async Task<IActionResult> Delete(PrivatePlugin<PluginVersion<string>> plugin, string versionId)
        {
            await _pluginRepository.RemovePluginVersion(plugin.Id, versionId);
            TempData["StatusMessage"] = "Success! Version was removed!";
            return Content($"Plugins/Edit/{plugin.Id}");
        }
    }
}
