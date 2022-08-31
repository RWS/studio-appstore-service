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

        public VersionController(IPluginRepository pluginRepository)
        {
            _pluginRepository = pluginRepository;
        }

        [HttpPost]
        public IActionResult Show(List<PluginVersion> versions, PluginDetailsModel pluginDetails)
        {
            var version = versions.FirstOrDefault(v => v.Id.Equals(pluginDetails.SelectedVersionId));
            ModelState.Clear();
            return PartialView("_PluginVersionDetailsPartial", version);
        }

        [HttpPost]
        public IActionResult Add()
        {
            var version = new PluginVersion
            {
                VersionName = "New plugin version",
                VersionNumber = string.Empty,
                IsPrivatePlugin = true,
                IsNewVersion = true,
                Id = Guid.NewGuid().ToString()
            };

            return PartialView("_PluginVersionDetailsPartial", version);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(PluginDetailsModel pluginDetails, string id)
        {
            var plugin = pluginDetails.PrivatePlugin;
            await _pluginRepository.RemovePluginVersion(plugin.Id, id);
            return RedirectToAction("Edit", "Plugins", new { id = plugin.Id, area = "Plugins" });
        }
    }
}
