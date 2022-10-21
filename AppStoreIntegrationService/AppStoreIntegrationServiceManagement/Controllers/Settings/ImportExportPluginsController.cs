using AppStoreIntegrationServiceCore.Model;
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
        private readonly IPluginRepositoryExtended<PluginDetails<PluginVersion<string>, string>> _pluginRepositoryExtended;
        private readonly IProductsRepository _productsRepository;
        private readonly IVersionProvider _versionProvider;
        private readonly ICategoriesRepository _categoriesRepository;

        public ImportExportPluginsController
        (
            IPluginRepositoryExtended<PluginDetails<PluginVersion<string>, string>> pluginRepositoryExtended, 
            IProductsRepository productsRepository, 
            IVersionProvider versionProvider, 
            ICategoriesRepository categoriesRepository
        )
        {
            _pluginRepositoryExtended = pluginRepositoryExtended;
            _productsRepository = productsRepository;
            _versionProvider = versionProvider;
            _categoriesRepository = categoriesRepository;
        }

        [Route("Settings/ExportPlugins")]
        public IActionResult Export()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateExport()
        {
            var response = new PluginResponse<PluginDetails<PluginVersion<string>, string>> 
            { 
                APIVersion = await _versionProvider.GetAPIVersion(),
                Value = await _pluginRepositoryExtended.GetAll("asc"),
                Products = await _productsRepository.GetAllProducts(),
                ParentProducts = await _productsRepository.GetAllParents(),
                Categories = await _categoriesRepository.GetAllCategories()
            };
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
                modalDetails.Title = "Warning!";
                modalDetails.Message = "The file is empty or in wrong format!";
                modalDetails.ModalType = ModalType.WarningMessage;
            }

            return PartialView("_ModalPartial", modalDetails);
        }
    }
}
