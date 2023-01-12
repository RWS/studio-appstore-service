using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model;
using AppStoreIntegrationServiceManagement.Model.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace AppStoreIntegrationServiceManagement.Controllers.Settings
{
    [Area("Settings")]
    public class ImportExportPluginsController : Controller
    {
        private readonly IPluginManager _pluginManager;
        private readonly IProductsManager _productsManager;
        private readonly IVersionManager _versionManager;
        private readonly ICategoriesManager _categoriesManager;
        private readonly IPluginRepository _pluginRepository;

        public ImportExportPluginsController
        (
            IProductsManager productsmanager,
            IVersionManager versionManager,
            ICategoriesManager categoriesManager,
            IPluginManager pluginManager,
            IPluginRepository pluginRepository
        )
        {
            _productsManager = productsmanager;
            _versionManager = versionManager;
            _categoriesManager = categoriesManager;
            _pluginManager = pluginManager;
            _pluginRepository = pluginRepository;
        }

        [Authorize(Roles = "Administrator, Developer")]
        [Route("Settings/ExportPlugins")]
        public IActionResult Export()
        {
            return View();
        }

        [Authorize(Roles = "Administrator, Developer")]
        [HttpPost]
        public async Task<IActionResult> CreateExport()
        {
            var response = new PluginResponse<PluginDetails>
            {
                APIVersion = await _versionManager.GetVersion(),
                Value = await _pluginRepository.GetAll("asc", User),
                Products = await _productsManager.ReadProducts(),
                ParentProducts = await _productsManager.ReadParents(),
                Categories = await _categoriesManager.ReadCategories()
            };

            var stream = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
            return File(stream, "application/octet-stream", "ExportPluginsConfig.json");
        }

        [Authorize(Roles = "Administrator")]
        [Route("Settings/ImportPlugins")]
        public IActionResult Import()
        {
            return View(new ImportPluginsModel());
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> CreateImport(ImportPluginsModel import)
        {
            var modalDetails = new ModalMessage
            {
                Title = "Warning!",
                Message = "The file is empty or in wrong format!",
                ModalType = ModalType.WarningMessage
            };

            var success = TryImportFromFile(import.ImportedFile, out var response);

            if (success)
            {
                await _categoriesManager.SaveCategories(response.Categories);
                await _productsManager.SaveProducts(response.Products);
                await _productsManager.SaveProducts(response.ParentProducts);
                await _versionManager.SaveVersion(response.APIVersion);
                await _pluginManager.SavePlugins(response.Value);

                modalDetails.RequestPage = "/Plugins";
                modalDetails.ModalType = ModalType.SuccessMessage;
                modalDetails.Title = "Success!";
                modalDetails.Message = $"The file content was imported! Return to plugins list?";
            }

            return PartialView("_ModalPartial", modalDetails);
        }

        private static bool TryImportFromFile(IFormFile file, out PluginResponse<PluginDetails> response)
        {
            var result = new StringBuilder();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                {
                    result.AppendLine(reader.ReadLine());
                }
            }

            if (!string.IsNullOrEmpty(result.ToString()))
            {
                try
                {
                    response = JsonConvert.DeserializeObject<PluginResponse<PluginDetails>>(result.ToString());
                    return true;

                }
                catch (JsonException)
                {
                    response = null;
                    return false;
                }
            }

            response = null;
            return false;
        }
    }
}
