using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceCore.Repository.V2.Interface;
using AppStoreIntegrationServiceManagement.Model;
using AppStoreIntegrationServiceManagement.Model.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace AppStoreIntegrationServiceManagement.Controllers.Settings
{
    [Authorize(Policy = "IsAdmin")]
    [Area("Settings")]
    public class ImportExportPluginsController : Controller
    {
        private readonly IPluginRepositoryExtended<PluginDetails<PluginVersion<string>>> _pluginRepositoryExtended;
        private readonly IProductsSynchronizer _productsSynchronizer;

        public ImportExportPluginsController(IPluginRepositoryExtended<PluginDetails<PluginVersion<string>>> pluginRepositoryExtended, IProductsSynchronizer productsSynchronizer)
        {
            _pluginRepositoryExtended = pluginRepositoryExtended;
            _productsSynchronizer = productsSynchronizer;
        }

        [Route("Settings/ExportPlugins")]
        public IActionResult Export()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateExport()
        {
            var response = new PluginResponse<PluginDetails<PluginVersion<string>>> { Value = await _pluginRepositoryExtended.GetAll("asc") };
            var jsonString = JsonConvert.SerializeObject(response);
            var stream = Encoding.UTF8.GetBytes(jsonString);
            return File(stream, "application/octet-stream", "ExportPluginsConfig.json");
        }

        [Route("Settings/ImportPlugins")]
        public IActionResult Import()
        {
            return View(new ImportPluginsModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateImport(ImportPluginsModel import)
        {
            var modalDetails = new ModalMessage();
            var success = await _pluginRepositoryExtended.TryImportPluginsFromFile(import.ImportedFile);
            
            if (success)
            {
                modalDetails.RequestPage = "/Plugins";
                modalDetails.ModalType = ModalType.SuccessMessage;
                modalDetails.Title = "Success!";
                modalDetails.Message = $"The file content was imported! Return to plugins list?";
            }
            else
            {
                modalDetails.Title = string.Empty;
                modalDetails.Message = "The file is empty or in wrong format!";
                modalDetails.ModalType = ModalType.WarningMessage;
            }

            return PartialView("_ModalPartial", modalDetails);
        }
    }
}
