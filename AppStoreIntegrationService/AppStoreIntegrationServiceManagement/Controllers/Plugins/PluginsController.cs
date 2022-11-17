using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceManagement.Controllers.Plugins
{
    [Area("Plugins")]
    public class PluginsController : Controller
    {
        private readonly IPluginRepository<PluginDetails<PluginVersion<string>, string>> _pluginRepository;
        private readonly IProductsRepository _productsRepository;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IHttpContextAccessor _context;
        private readonly string _pluginDownloadPath;

        public PluginsController
        (
            IPluginRepository<PluginDetails<PluginVersion<string>, string>> pluginRepository,
            IHttpContextAccessor context,
            IProductsRepository productsRepository,
            ICategoriesRepository categoriesRepository,
            IWebHostEnvironment environment
        )
        {
            _pluginRepository = pluginRepository;
            _context = context;
            _productsRepository = productsRepository;
            _categoriesRepository = categoriesRepository;
            _pluginDownloadPath = $@"{environment.ContentRootPath}\Temp";
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
                Plugins = InitializePrivatePlugins(_pluginRepository.SearchPlugins(pluginsList, pluginsFilters, products)).ToList(),
                ProductsListItems = new SelectList(products ?? new List<ProductDetails>(), nameof(ProductDetails.Id), nameof(ProductDetails.ProductName)),
                StatusExists = Request.Query.TryGetValue("status", out var statusValue),
                StatusValue = statusValue,
                SearchExists = Request.Query.TryGetValue("search", out var searchValue),
                SearchValue = searchValue,
                ProductExists = Request.Query.TryGetValue("product", out var productValue),
                ProductName = products?.FirstOrDefault(p => p.Id == productValue)?.ProductName
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
        public async Task<IActionResult> ManifestCompare(PrivatePlugin<PluginVersion<string>> plugin, ExtendedPluginVersion<string> version)
        {
            if (!Directory.Exists(_pluginDownloadPath))
            {
                Directory.CreateDirectory(_pluginDownloadPath);
            }

            try
            {
                await DownloadPlugin(version.DownloadUrl);
                ZipFile.ExtractToDirectory($@"{_pluginDownloadPath}\Plugin.sdlplugin", _pluginDownloadPath);
                var response = ImportFromFile($@"{_pluginDownloadPath}\pluginpackage.manifest.xml");

                var isNameMatch = response.PluginName == plugin.Name;
                var isVersionMatch = response.Version == version.VersionNumber;
                var isMinVersionMatch = response.RequiredProduct.MinimumStudioVersion == version.MinimumRequiredVersionOfStudio;
                var isMaxVersionMatch = response.RequiredProduct.MaximumStudioVersion == version.MaximumRequiredVersionOfStudio;
                var isAuthorMatch = response.Author == plugin.DeveloperName;
                //var isProductMatch = (await _productsRepository.GetAllProducts()).FirstOrDefault(p => p.Id == version.SelectedProductId)?.MinimumStudioVersion == version.MinimumRequiredVersionOfStudio;
                var isFullMatch = new[] { isNameMatch, isVersionMatch, isMinVersionMatch, isMaxVersionMatch, isAuthorMatch}.All(match => match);
                Directory.Delete(_pluginDownloadPath, true);

                TempData["ManifestCompare"] = new { isNameMatch, isVersionMatch, isMinVersionMatch, isMaxVersionMatch, isAuthorMatch, isFullMatch };
                if (isFullMatch)
                {
                    return PartialView("_StatusMessage", "Success! The comparison finished without conflicts!");
                }

                return PartialView("_StatusMessage", "Error! The comparison finished with conflicts!");
            }
            catch (InvalidDataException)
            {
                Directory.Delete(_pluginDownloadPath, true);
                return PartialView("_StatusMessage", "Error! The extract doesn't contain a manifest file!");
            }
            catch (Exception e)
            {
                Directory.Delete(_pluginDownloadPath, true);
                return PartialView("_StatusMessage", $"Error! {e.Message}");
            }
        }

        private async Task DownloadPlugin(string downloadUrl)
        {
            try
            {
                var reader = new RemoteStreamReader(new Uri(downloadUrl));
                var stream = await reader.ReadAsync();
                using var fileStream = System.IO.File.Create($@"{_pluginDownloadPath}\Plugin.sdlplugin");
                await stream.CopyToAsync(fileStream);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
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

        private static PluginPackage ImportFromFile(string file)
        {
            var serializer = new XmlSerializer(typeof(PluginPackage));
            var result = new StringBuilder();
            using (var reader = new StreamReader(file))
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
                    return (PluginPackage)serializer.Deserialize(new XmlReaderNamespaceIgnore(new StringReader(result.ToString())));
                }
                catch (XmlException)
                {
                    return null;
                }

            }

            return null;
        }

        private async Task<bool> IsSaved(PrivatePlugin<PluginVersion<string>> plugin, ExtendedPluginVersion<string> version)
        {
            var foundPluginDetails = await _pluginRepository.GetPluginById(plugin.Id);
            var newPluginDetails = plugin.ConvertToPluginDetails(foundPluginDetails, version);
            return JsonConvert.SerializeObject(newPluginDetails) == JsonConvert.SerializeObject(foundPluginDetails);
        }

        private async Task<IActionResult> Save
        (
            PrivatePlugin<PluginVersion<string>> plugin,
            List<ExtendedPluginVersion<string>> versions,
            ExtendedPluginVersion<string> version,
            Func<PrivatePlugin<PluginVersion<string>>, Task> func
        )
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
                    SelectedProductIds = products.Select(p => p.Id).ToList(),
                    SelectedProduct = lastSupportedProduct,
                    VersionName = $"{lastSupportedProduct.ProductName} - {version.VersionNumber}",
                });
            }

            return newVersions;
        }

        private IEnumerable<PrivatePlugin<PluginVersion<string>>> InitializePrivatePlugins(List<PluginDetails<PluginVersion<string>, string>> plugins)
        {
            if (plugins == null)
            {
                yield break;
            }

            foreach (var plugin in plugins)
            {
                yield return new PrivatePlugin<PluginVersion<string>>
                {
                    Id = plugin.Id,
                    Description = plugin.Description,
                    Name = plugin.Name,
                    Categories = plugin.Categories,
                    Versions = plugin.Versions?.Select(v => new ExtendedPluginVersion<string>(v)).ToList(),
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
