using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Settings
{
    [Authorize(Policy = "IsAdmin")]
    [Route("Settings/[controller]")]
    public class ImportPluginsController : Controller
    {
        private readonly IPluginRepository _pluginRepository;

        public ImportPluginsController(IPluginRepository pluginRepository)
        {
            _pluginRepository = pluginRepository;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return View("Views/Settings/ImportPlugins.cshtml", new ImportPluginsModel());
        }

        [HttpPost]
        public async Task<IActionResult> Import(ImportPluginsModel import)
        {
            var modalDetails = new ModalMessage();
            var success = await _pluginRepository.TryImportPluginsFromFile(import.ImportedFile);
            if (success)
            {
                modalDetails.RequestPage = "/ConfigTool";
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

            return PartialView("/Views/_ModalPartial.cshtml", modalDetails);
        }
    }
}
