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

        [HttpPost]
        public IActionResult Show(List<PluginVersion> versions, PluginDetailsModel pluginDetails)
        {
            var version = versions.FirstOrDefault(v => v.Id.Equals(pluginDetails.SelectedVersionId));
            ModelState.Clear();
            return PartialView("_PluginVersionDetailsPartial", version);
        }

        [HttpPost]
        public async Task<IActionResult> Add()
        {
            var version = new PluginVersion((await _productsRepository.GetAllProducts()).ToList())
            {
                VersionName = "New plugin version",
                VersionNumber = string.Empty,
                IsPrivatePlugin = true,
                IsNewVersion = true,
                Id = Guid.NewGuid().ToString()
            };

            version.SetSupportedProducts();
            return PartialView("_PluginVersionDetailsPartial", version);
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
