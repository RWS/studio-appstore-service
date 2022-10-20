using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.V2.Interface;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Plugins
{
    [Area("Plugins")]
    public class VersionController : Controller
    {
        private readonly IPluginRepositoryExtended<PluginDetails<PluginVersion<string>>> _pluginRepository;
        private readonly IProductsRepository _productsRepository;

        public VersionController(IPluginRepositoryExtended<PluginDetails<PluginVersion<string>>> pluginRepository, IProductsRepository productsRepository)
        {
            _pluginRepository = pluginRepository;
            _productsRepository = productsRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Show(List<ExtendedPluginVersion<string>> versions, PluginDetailsModel pluginDetails)
        {
            var version = versions.FirstOrDefault(v => v.Id.Equals(pluginDetails.SelectedVersionId));
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
                Id = Guid.NewGuid().ToString()
            };

            version.SetSupportedProductsList(products, products.FirstOrDefault(x => x.IsDefault)?.Id);

            return PartialView("_PluginVersionDetailsPartial", version);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateChecksum(ExtendedPluginVersion<string> version)
        {
            try
            {
                var remoteReader = new RemoteStreamReader(new Uri(version.DownloadUrl));
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
        public async Task<IActionResult> Delete(PluginDetailsModel pluginDetails, string id)
        {
            var plugin = pluginDetails.PrivatePlugin;
            await _pluginRepository.RemovePluginVersion(plugin.Id, id);
            TempData["StatusMessage"] = "Success! Version was removed!";
            return Content($"Plugins/Edit/{plugin.Id}");
        }
    }
}
