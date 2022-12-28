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
        private readonly IPluginRepository _pluginRepository;
        private readonly IProductsRepository _productsRepository;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IHttpContextAccessor _context;
        private readonly string _pluginDownloadPath;

        public PluginsController
        (
            IPluginRepository pluginRepository,
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
            PluginFilter filter = ApplyFilters();
            var pluginsList = await _pluginRepository.GetAll(filter.SortOrder, User.IsInRole("Developer") ? User.Identity.Name : null);
            var products = await _productsRepository.GetAllProducts();
            var statusFilters = new List<string> { "Active", "Inactive" };

            if (User.IsInRole("Developer"))
            {
                statusFilters.Add("Draft");
                statusFilters.Add("InReview");
            }

            if (User.IsInRole("Administrator"))
            {
                statusFilters.Add("InReview");
            }

            var status = statusFilters.Select(x => new KeyValuePair<FilterType, FilterItem>(FilterType.Status, new FilterItem
            {
                Id = $"{(int)Enum.Parse(typeof(Status), x)}",
                Name = x,
                IsSelected = Request.Query["Status"].Any(y => y == $"{(int)Enum.Parse(typeof(Status), x)}")
            }));

            return View(new ConfigToolModel
            {
                Plugins = _pluginRepository.SearchPlugins(pluginsList, filter, products).Select(p => new ExtendedPluginDetails(p)).ToList(),
                StatusListItems = new SelectList(status.Select(x => x.Value), nameof(FilterItem.Id), nameof(FilterItem.Name), Request.Query["Status"].FirstOrDefault()),
                ProductsListItems = new SelectList(products ?? new List<ProductDetails>(), nameof(ProductDetails.Id), nameof(ProductDetails.ProductName), Request.Query["Product"].FirstOrDefault()),
                Filters = status.Concat(products.Select(x => new KeyValuePair<FilterType, FilterItem>(FilterType.Status, new FilterItem
                {
                    Id = x.Id,
                    Name = x.ProductName,
                    IsSelected = Request.Query["Product"].Any(y => y == x.Id)
                }))).Append(new KeyValuePair<FilterType, FilterItem>(FilterType.Query, new FilterItem
                {
                    Id = "0",
                    Name = Request.Query["q"].FirstOrDefault(),
                    IsSelected = Request.Query["q"].FirstOrDefault(x => !string.IsNullOrEmpty(x)) != null
                }))
            });
        }

        [Route("Plugins/New")]
        public async Task<IActionResult> New()
        {
            var categories = await _categoriesRepository.GetAllCategories();
            return View("Details", new ExtendedPluginDetails
            {
                Icon = new IconDetails { MediaUrl = GetDefaultIcon() },
                Developer = new DeveloperDetails { DeveloperName = User.IsInRole("Administrator") ? "" : User.Identity.Name },
                IsEditMode = false,
                SelectedVersionId = Guid.NewGuid().ToString(),
                CategoryListItems = new MultiSelectList(categories, nameof(CategoryDetails.Id), nameof(CategoryDetails.Name))
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(ExtendedPluginDetails plugin, Status status)
        {
            return await Save(plugin, status, _pluginRepository.AddPlugin);
        }

        [Route("Plugins/Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var categories = await _categoriesRepository.GetAllCategories();
            var plugin = await _pluginRepository.GetPluginById(id, User.IsInRole("Developer") ? User.Identity.Name : null);

            if (plugin == null)
            {
                return NotFound();
            }

            return View("Details", new ExtendedPluginDetails(plugin)
            {
                IsEditMode = true,
                CategoryListItems = new MultiSelectList(categories, nameof(CategoryDetails.Id), nameof(CategoryDetails.Name))
            });
        }

        [HttpPost]
        public async Task<IActionResult> Update(ExtendedPluginDetails plugin, Status status)
        {
            return await Save(plugin, status, _pluginRepository.UpdatePlugin);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _pluginRepository.RemovePlugin(id);
            TempData["StatusMessage"] = "Success! Plugin was removed!";
            return Content("");
        }

        [HttpPost]
        public async Task<IActionResult> ManifestCompare(ExtendedPluginDetails plugin, ExtendedPluginVersion version)
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
                Directory.Delete(_pluginDownloadPath, true);
                TempData["ManifestCompare"] = response.CreateMatchLog(plugin, version, await _productsRepository.GetAllProducts(), out bool isFullMatch);
                return PartialView("_StatusMessage", string.Format("{0}! The comparison finished with{1} conflicts!", isFullMatch ? "Success" : "Error", isFullMatch ? "out" : ""));
            }
            catch (Exception e)
            {
                Directory.Delete(_pluginDownloadPath, true);
                if (e is InvalidDataException || e is FileNotFoundException)
                {
                    return PartialView("_StatusMessage", "Error! The extract doesn't contain a manifest file!");
                }

                return PartialView("_StatusMessage", $"Error! {e.Message}");
            }
        }

        private async Task DownloadPlugin(string downloadUrl)
        {
            try
            {
                var reader = new RemoteStreamReader(new Uri(downloadUrl));
                var stream = await reader.ReadAsStreamAsync();
                using var fileStream = System.IO.File.Create($@"{_pluginDownloadPath}\Plugin.sdlplugin");
                await stream.CopyToAsync(fileStream);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        [Route("[controller]/[action]/{redirectUrl}/{currentPage}")]
        public async Task<IActionResult> GoToPage(ExtendedPluginDetails plugin, ExtendedPluginVersion version, string redirectUrl, string currentPage)
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

        private async Task<bool> IsSaved(ExtendedPluginDetails plugin, ExtendedPluginVersion version)
        {
            var foundPluginDetails = await _pluginRepository.GetPluginById(plugin.Id);
            var newPluginDetails = plugin.ConvertToPluginDetails(foundPluginDetails, version);
            return JsonConvert.SerializeObject(newPluginDetails) == JsonConvert.SerializeObject(foundPluginDetails);
        }

        private async Task<IActionResult> Save(ExtendedPluginDetails plugin, Status status, Func<PluginDetails<PluginVersion<string>, string>, Task> func)
        {
            try
            {
                plugin.Status = status;
                await func(new PluginDetails<PluginVersion<string>, string>(plugin));
                TempData["StatusMessage"] = string.Format("Success! {0} was {1}!", plugin.Name, plugin.IsEditMode ? "updated" : "saved");
                return Content($"/Plugins/Edit/{plugin.Id}");
            }
            catch (Exception e)
            {
                return PartialView("_StatusMessage", string.Format("Error! {0}!", e.Message));
            }
        }

        private PluginFilter ApplyFilters()
        {
            const string statusFilter = "Status";
            const string searchFilter = "q";
            const string productFilter = "Product";
            var query = Request.Query;
            var filters = new PluginFilter()
            {
                SortOrder = "asc",
                Status = Status.All,
                SupportedProduct = query.ContainsKey(productFilter) ? query[productFilter] : default,
                Query = query.ContainsKey(searchFilter) ? query[searchFilter] : default
            };

            if (query.ContainsKey(statusFilter))
            {
                bool isValidType = int.TryParse(query[statusFilter], out int statusValueIndex);
                if (isValidType && Enum.IsDefined(typeof(Status), statusValueIndex))
                {
                    filters.Status = (Status)statusValueIndex;
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
