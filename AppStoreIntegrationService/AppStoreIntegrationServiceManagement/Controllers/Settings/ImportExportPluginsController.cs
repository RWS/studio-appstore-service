﻿using AppStoreIntegrationServiceCore.Model;
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
        private readonly IPluginManager _pluginManager;
        private readonly IProductsRepository _productsRepository;
        private readonly IVersionManager _versionManager;
        private readonly ICategoriesManager _categoriesManager;

        public ImportExportPluginsController
        (
            IProductsRepository productsRepository,
            IVersionManager versionManager,
            ICategoriesManager categoriesManager,
            IPluginManager pluginManager
        )
        {
            _productsRepository = productsRepository;
            _versionManager = versionManager;
            _categoriesManager = categoriesManager;
            _pluginManager = pluginManager;
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
                APIVersion = await _versionManager.GetVersion(),
                Value = await _pluginManager.GetPlugins(),
                Products = await _productsRepository.GetAllProducts(),
                ParentProducts = await _productsRepository.GetAllParents(),
                Categories = await _categoriesManager.ReadCategories()
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
                await _categoriesManager.SaveCategories(response.Categories);
                await _productsRepository.UpdateProducts(response.Products);
                await _productsRepository.UpdateProducts(response.ParentProducts);
                await _versionManager.SaveVersion(response.APIVersion);
                await _pluginManager.SavePlugins(response.Value);

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
