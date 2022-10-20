using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.V2.Interface;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Plugins
{
    [Area("Plugins")]
    public class VersionController : Controller
    {
        private readonly IPluginRepositoryExtended<PluginDetails<PluginVersion<string>, string>> _pluginRepository;
        private readonly IProductsRepository _productsRepository;

        public VersionController(IPluginRepositoryExtended<PluginDetails<PluginVersion<string>, string>> pluginRepository, IProductsRepository productsRepository)
        {
            _pluginRepository = pluginRepository;
            _productsRepository = productsRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Show(List<ExtendedPluginVersion<string>> versions, PrivatePlugin<PluginVersion<string>> plugin)
        {
            var version = versions.FirstOrDefault(v => v.VersionId.Equals(plugin.SelectedVersionId));
            var products = (await _productsRepository.GetAllProducts()).ToList();
            version.SetSupportedProductsList(products, version.SelectedProduct.Id);
            return PartialView("_PluginVersionDetailsPartial", version);
        }

        [HttpPost]
        public async Task<IActionResult> Add()
        {
            var products = (await _productsRepository.GetAllProducts()).ToList();
            var version = new ExtendedPluginVersion<string>
            {
                VersionName = "New plugin version",
                VersionNumber = string.Empty,
                IsPrivatePlugin = true,
                IsNewVersion = true,
                VersionId = Guid.NewGuid().ToString()
            };

            version.SetSupportedProductsList(products, products.FirstOrDefault(x => x.IsDefault)?.Id);

            return PartialView("_PluginVersionDetailsPartial", version);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateChecksum(ExtendedPluginVersion<string> version)
        {
            try
            {
                var remoteReader = new RemoteStreamReader(new Uri(version.VersionDownloadUrl));
                version.FileHash = SHA1Generator.GetHash(await remoteReader.ReadAsync());
            }
            catch (Exception e)
            {
                return PartialView("_StatusMessage", $"Error! {e.Message}");
            }

            TempData["Filehash"] = version.FileHash;
            return PartialView("_StatusMessage", "Success! Checksum was computed!");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(PrivatePlugin<PluginVersion<string>> plugin, string id)
        {
            await _pluginRepository.RemovePluginVersion(plugin.Id, id);
            TempData["StatusMessage"] = "Success! Version was removed!";
            return Content($"Plugins/Edit/{plugin.Id}");
        }
    }
}
