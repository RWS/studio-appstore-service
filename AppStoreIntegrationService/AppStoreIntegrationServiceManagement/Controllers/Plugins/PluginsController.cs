using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using static AppStoreIntegrationServiceCore.Enums;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace AppStoreIntegrationServiceManagement.Controllers.Plugins
{
    [Area("Plugins")]
    public class PluginsController : Controller
    {
        private readonly IPluginRepository<PluginDetails<PluginVersion<string>, string>> _pluginRepository;
        private readonly IProductsRepository _productsRepository;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IHttpContextAccessor _context;

        public PluginsController
        (
            IPluginRepository<PluginDetails<PluginVersion<string>, string>> pluginRepository,
            IHttpContextAccessor context,
            IProductsRepository productsRepository,
            ICategoriesRepository categoriesRepository
        )
        {
            _pluginRepository = pluginRepository;
            _context = context;
            _productsRepository = productsRepository;
            _categoriesRepository = categoriesRepository;
        }

        [Route("Plugins")]
        [Route("/")]
        public async Task<IActionResult> Index()
        {
            PluginFilter pluginsFilters = ApplyFilters();
            var pluginsList = await _pluginRepository.GetAll(pluginsFilters.SortOrder);
            var products = await _productsRepository.GetAllProducts();
            return View(new ConfigToolModel
            {
                Plugins = InitializePrivatePlugins(_pluginRepository.SearchPlugins(pluginsList, pluginsFilters)).ToList(),
                ProductsListItems = new SelectList(products, nameof(ProductDetails.Id), nameof(ProductDetails.ProductName)),
                StatusExists = Request.Query.TryGetValue("status", out var statusValue),
                StatusValue = statusValue,
                SearchExists = Request.Query.TryGetValue("search", out var searchValue),
                SearchValue = searchValue,
                ProductExists = Request.Query.TryGetValue("product", out var productValue),
                ProductName = products.FirstOrDefault(p => p.Id == productValue)?.ProductName
            });
        }

        [Route("Plugins/New")]
        public async Task<IActionResult> New()
        {
            var categories = await _categoriesRepository.GetAllCategories();
            return View(new PrivatePlugin<PluginVersion<string>>
            {
                IconUrl = GetDefaultIcon(),
                IsEditMode = false,
                SelectedVersionId = Guid.NewGuid().ToString(),
                CategoryListItems = new MultiSelectList(categories, nameof(CategoryDetails.Id), nameof(CategoryDetails.Name))
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(PrivatePlugin<PluginVersion<string>> plugin, List<ExtendedPluginVersion<string>> versions, ExtendedPluginVersion<string> version)
        {
            return await Save(plugin, versions, version, _pluginRepository.AddPrivatePlugin);
        }

        [Route("Plugins/Edit/{id?}")]
        public async Task<IActionResult> Edit(int id)
        {
            var categories = await _categoriesRepository.GetAllCategories();
            var pluginDetails = await _pluginRepository.GetPluginById(id);
            return View(new PrivatePlugin<PluginVersion<string>>
            {
                Id = pluginDetails.Id,
                PaidFor = pluginDetails.PaidFor,
                DeveloperName = pluginDetails.Developer?.DeveloperName,
                Description = pluginDetails.Description,
                Name = pluginDetails.Name,
                ChangelogLink = pluginDetails.ChangelogLink,
                SupportEmail = pluginDetails.SupportEmail,
                SupportUrl = pluginDetails.SupportUrl,
                Categories = pluginDetails.Categories,
                Inactive = pluginDetails.Inactive,
                Versions = SetSelectedProducts(pluginDetails.Versions).ToList(),
                IconUrl = string.IsNullOrEmpty(pluginDetails.Icon.MediaUrl) ? GetDefaultIcon() : pluginDetails.Icon.MediaUrl,
                IsEditMode = true,
                CategoryListItems = new MultiSelectList(categories, nameof(CategoryDetails.Id), nameof(CategoryDetails.Name))
            });
        }

        [HttpPost]
        public async Task<IActionResult> Update(PrivatePlugin<PluginVersion<string>> plugin, List<ExtendedPluginVersion<string>> versions, ExtendedPluginVersion<string> version)
        {
            return await Save(plugin, versions, version, _pluginRepository.UpdatePrivatePlugin);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _pluginRepository.RemovePlugin(id);
            TempData["StatusMessage"] = "Success! Plugin was removed!";
            return Content("");
        }

        [HttpPost]
        public IActionResult ManifestCompare(PrivatePlugin<PluginVersion<string>> plugin, ExtendedPluginVersion<string> version, ImportManifestModel manifest)
        {
            _ = TryImportFromFile(manifest.ManifestFile, out PluginPackage response);

            if (IsManifestMatch(plugin, response, version, out var match))
            {
                return PartialView("_StatusMessage", "Success! Manifest and plugin data matches!");
            }

            TempData["ManifestComparison"] = match;
            return PartialView("_StatusMessage", "Error! Manifest and plugin data do not match!");
        }

        private static bool IsManifestMatch (PrivatePlugin<PluginVersion<string>> plugin, PluginPackage response, ExtendedPluginVersion<string> version, out Tuple<bool, bool, bool, bool> match)
        {
            match = Tuple.Create(
                response.PluginName == plugin.Name,
                response.Version == version.VersionNumber,
                response.RequiredProduct.MinimumStudioVersion == version.MinimumRequiredVersionOfStudio,
                response.RequiredProduct.MaximumStudioVersion == version.MinimumRequiredVersionOfStudio
            );

            return match.Item1 && match.Item2 && match.Item3 && match.Item4;
        }

        private static bool TryImportFromFile(IFormFile file, out PluginPackage response)
        {
            var serializer = new XmlSerializer(typeof(PluginPackage));
            var result = new StringBuilder();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                {
                    result.AppendLine(reader.ReadLine());
                }
            }

            if (result.Length > 0)
            {
                try
                {
                    response = (PluginPackage)serializer.Deserialize(new XmlReaderNamespaceIgnore(new StringReader(result.ToString())));
                    return true;
                }
                catch (XmlException)
                {
                    response = null;
                    return false;
                }

            }

            response = null;
            return false;
        }

        [Route("[controller]/[action]/{redirectUrl}/{currentPage}")]
        public async Task<IActionResult> GoToPage(PrivatePlugin<PluginVersion<string>> plugin, ExtendedPluginVersion<string> version, string redirectUrl, string currentPage)
        {
            redirectUrl = redirectUrl.Replace('.', '/');

            if (currentPage != "New" && await IsSaved(plugin, version))
            {
                return Content($"{redirectUrl}");
            }

            var modalDetails = new ModalMessage
            {
                RequestPage = $"{redirectUrl}",
                ModalType = ModalType.WarningMessage,
                Title = "Unsaved changes!",
                Message = string.Format("Discard changes for {0}?", string.IsNullOrEmpty(plugin.Name) ? "plugin" : plugin.Name)
            };

            return PartialView("_ModalPartial", modalDetails);
        }

        private async Task<bool> IsSaved(PrivatePlugin<PluginVersion<string>> plugin, ExtendedPluginVersion<string> version)
        {
            var foundPluginDetails = await _pluginRepository.GetPluginById(plugin.Id);
            var newPluginDetails = plugin.ConvertToPluginDetails(foundPluginDetails, version);
            return JsonConvert.SerializeObject(newPluginDetails) == JsonConvert.SerializeObject(foundPluginDetails);
        }

        private async Task<IActionResult> Save(PrivatePlugin<PluginVersion<string>> plugin, List<ExtendedPluginVersion<string>> versions, ExtendedPluginVersion<string> version, Func<PrivatePlugin<PluginVersion<string>>, Task> func)
        {
            if (plugin.IsValid(version))
            {
                plugin.SetVersionList(versions, version);
                plugin.SetDownloadUrl();

                try
                {
                    await func(plugin);
                    TempData["StatusMessage"] = string.Format("Success! {0} was {1}!", plugin.Name, plugin.IsEditMode ? "updated" : "saved");
                    return Content($"/Plugins/Edit/{plugin.Id}");
                }
                catch (Exception e)
                {
                    return PartialView("_StatusMessage", string.Format("Error! {0}!", e.Message));
                }
            }

            return PartialView("_StatusMessage", "Error! Please fill all required values!");
        }

        private IEnumerable<ExtendedPluginVersion<string>> SetSelectedProducts(List<PluginVersion<string>> versions)
        {
            var products = _productsRepository.GetAllProducts().Result;
            var newVersions = new List<ExtendedPluginVersion<string>>();
            foreach (var version in versions)
            {
                var lastSupportedProduct = products.FirstOrDefault(p => p.Id == version.SupportedProducts[0]);
                newVersions.Add(new ExtendedPluginVersion<string>(version)
                {
                    SelectedProductId = lastSupportedProduct.Id,
                    SelectedProduct = lastSupportedProduct,
                    VersionName = $"{lastSupportedProduct.ProductName} - {version.VersionNumber}",
                });
            }

            return newVersions;
        }

        private IEnumerable<PrivatePlugin<PluginVersion<string>>> InitializePrivatePlugins(List<PluginDetails<PluginVersion<string>, string>> plugins)
        {
            foreach (var plugin in plugins)
            {
                yield return new PrivatePlugin<PluginVersion<string>>
                {
                    Id = plugin.Id,
                    Description = plugin.Description,
                    Name = plugin.Name,
                    Categories = plugin.Categories,
                    Versions = plugin.Versions.Select(v => new ExtendedPluginVersion<string>(v)).ToList(),
                    Inactive = plugin.Inactive,
                    IconUrl = string.IsNullOrEmpty(plugin.Icon.MediaUrl) ? GetDefaultIcon() : plugin.Icon.MediaUrl
                };
            }
        }

        private PluginFilter ApplyFilters()
        {
            const string statusFilter = "status";
            const string searchFilter = "search";
            const string productFilter = "product";
            var query = Request.Query;
            var filters = new PluginFilter()
            {
                SortOrder = "asc",
                Status = StatusValue.All,
                SupportedProduct = query.ContainsKey(productFilter) ? query[productFilter] : default,
                Query = query.ContainsKey(searchFilter) ? query[searchFilter] : default
            };

            if (query.ContainsKey(statusFilter))
            {
                bool isValidType = int.TryParse(query[statusFilter], out int statusValueIndex);
                if (isValidType && Enum.IsDefined(typeof(StatusValue), statusValueIndex))
                {
                    filters.Status = (StatusValue)statusValueIndex;
                }
            }

            return filters;
        }

        private string GetDefaultIcon()
        {
            var scheme = _context.HttpContext?.Request?.Scheme;
            var host = _context.HttpContext?.Request?.Host.Value;
            return $"{scheme}://{host}/images/plugin.ico";
        }
    }
}
