using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceManagement.Controllers.Plugins
{
    [Authorize]
    [Area("Plugins")]
    public class PluginsController : Controller
    {
        private readonly IPluginRepository _pluginRepository;
        private readonly IProductsRepository _productsRepository;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IHttpContextAccessor _context;

        public PluginsController
        (
            IPluginRepository pluginRepository,
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
        public async Task<IActionResult> Delete(int id)
        {
            await _pluginRepository.RemovePlugin(id);
            TempData["StatusMessage"] = "Success! Plugin was removed!";
            return Content("");
        }

        [HttpPost]
        public async Task<IActionResult> Save(ExtendedPluginDetails plugin, Status status)
        {
            try
            {
                plugin.Status = status;
                if (plugin.IsEditMode)
                {
                    await _pluginRepository.UpdatePlugin(new PluginDetails<PluginVersion<string>, string>(plugin));

                    if (plugin.DownloadUrl != null)
                    {
                        var response = await PluginPackage.DownloadPlugin(plugin.DownloadUrl);
                        TempData["ManifestPluginCompare"] = response.CreatePluginMatchLog(plugin, out bool isFullMatch);

                        if (!isFullMatch)
                        {
                            return PartialView("_StatusMessage", "Warning! Plugin was saved but there are manifest conflicts!");
                        }
                    }
                }
                else
                {
                    await _pluginRepository.AddPlugin(new PluginDetails<PluginVersion<string>, string>(plugin));
                }

                TempData["StatusMessage"] = $"Success! {plugin.Name} was saved!";
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
