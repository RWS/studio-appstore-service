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
    [Authorize(Policy = "IsAdmin")]
    [Area("Settings")]
    public class ImportExportPluginsController : Controller
    {
        private readonly IPluginRepository _pluginRepository;
        private readonly IProductsRepository _productsRepository;
        private readonly IVersionProvider _versionProvider;
        private readonly ICategoriesRepository _categoriesRepository;

        public ImportExportPluginsController
        (
            IPluginRepository pluginRepository,
            IProductsRepository productsRepository,
            IVersionProvider versionProvider,
            ICategoriesRepository categoriesRepository
        )
        {
            _pluginRepository = pluginRepository;
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
                Value = await _pluginRepository.GetAll("asc"),
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
            var modalDetails = new ModalMessage
            {
                Title = "Warning!",
                Message = "The file is empty or in wrong format!",
                ModalType = ModalType.WarningMessage
            };

            var success = TryImportFromFile(import.ImportedFile, out var response);

            if (success)
            {
                await _categoriesRepository.UpdateCategories(response.Categories);
                await _productsRepository.UpdateProducts(response.Products);
                await _productsRepository.UpdateProducts(response.ParentProducts);
                await _versionProvider.UpdateAPIVersion(response.APIVersion);
                await _pluginRepository.SaveToFile(response.Value);

                modalDetails.RequestPage = "/Plugins";
                modalDetails.ModalType = ModalType.SuccessMessage;
                modalDetails.Title = "Success!";
                modalDetails.Message = $"The file content was imported! Return to plugins list?";
            }

            return PartialView("_ModalPartial", modalDetails);
        }

        private static bool TryImportFromFile(IFormFile file, out PluginResponse<PluginDetails<PluginVersion<string>, string>> response)
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
                    response = JsonConvert.DeserializeObject<PluginResponse<PluginDetails<PluginVersion<string>, string>>>(result.ToString());
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
