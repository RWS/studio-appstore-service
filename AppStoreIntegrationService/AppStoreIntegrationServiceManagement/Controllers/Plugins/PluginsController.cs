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
        private readonly ICommentsRepository _commentsRepository;
        private readonly ILoggingRepository _loggingRepository;
        private readonly IHttpContextAccessor _context;

        public PluginsController
        (
            IPluginRepository pluginRepository,
            IHttpContextAccessor context,
            IProductsRepository productsRepository,
            ICategoriesRepository categoriesRepository,
            ICommentsRepository commentsRepository,
            ILoggingRepository loggingRepository
        )
        {
            _pluginRepository = pluginRepository;
            _context = context;
            _productsRepository = productsRepository;
            _categoriesRepository = categoriesRepository;
            _commentsRepository = commentsRepository;
            _loggingRepository = loggingRepository;
        }

        [Route("Plugins")]
        [Route("/")]
        public async Task<IActionResult> Index()
        {
            PluginFilter filter = ApplyFilters();
            var plugins = await _pluginRepository.GetAll(filter.SortOrder, User);
            var products = await _productsRepository.GetAllProducts();
            var status = new List<string> { "Active", "Inactive" }.Concat(User.IsInRole("StandardUser") ? new List<string>() : new[] { "Draft", "InReview" }).Select(x => new FilterItem
            {
                Id = "Status",
                Label = x,
                Value = $"{(int)Enum.Parse(typeof(Status), x)}",
                IsSelected = Request.Query["Status"].Any(y => y == $"{(int)Enum.Parse(typeof(Status), x)}")
            });

            return View(new ConfigToolModel
            {
                Plugins = PluginFilter.SearchPlugins(plugins, filter, products).Select(p => ExtendedPluginDetails.CopyFrom(p)),
                StatusListItems = new SelectList(status, nameof(FilterItem.Value), nameof(FilterItem.Label), Request.Query["Status"].FirstOrDefault()),
                ProductsListItems = new SelectList(products, nameof(ProductDetails.Id), nameof(ProductDetails.ProductName), Request.Query["Product"].FirstOrDefault()),
                Filters = status.Concat(products.Select(x => new FilterItem
                {
                    Id = "Product",
                    Label = x.ProductName,
                    Value = x.ProductName,
                    IsSelected = Request.Query["Product"].Any(y => y == x.Id)
                })).Append(new FilterItem
                {
                    Id = "Query",
                    Label = Request.Query["Query"],
                    Value = Request.Query["Query"],
                    IsSelected = !string.IsNullOrEmpty(Request.Query["Query"].FirstOrDefault())
                })
            });
        }

        [Route("Plugins/New")]
        public async Task<IActionResult> New()
        {
            var categories = await _categoriesRepository.GetAllCategories();
            return View("Details", new ExtendedPluginDetails
            {
                Icon = new IconDetails { MediaUrl = GetDefaultIcon() },
                Developer = new DeveloperDetails { DeveloperName = User.IsInRole("Administrator") ? null : User.Identity.Name },
                IsEditMode = false,
                IsThirdParty = User.IsInRole("Developer"),
                CategoryListItems = new MultiSelectList(categories, nameof(CategoryDetails.Id), nameof(CategoryDetails.Name))
            });
        }

        [Route("Plugins/Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var categories = await _categoriesRepository.GetAllCategories();
            var plugin = await _pluginRepository.GetPluginById(id, User);

            if (plugin == null)
            {
                return NotFound();
            }

            var isReadonly = User.IsInRole("StandardUser") && plugin.IsThirdParty;
            var extended = ExtendedPluginDetails.CopyFrom(plugin);
            extended.CategoryListItems = new MultiSelectList(categories, nameof(CategoryDetails.Id), nameof(CategoryDetails.Name));
            extended.IsEditMode = true;

            return View(isReadonly ? "ReadonlyDetails" : "Details", extended);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, bool deletionApproved)
        {
            var plugin = await _pluginRepository.GetPluginById(id, User);

            if (User.IsInRole("Developer") && plugin.HasAdminConsent && plugin.WasActive || 
               (User.IsInRole("Administrator") && !deletionApproved))
            {
                plugin.NeedsDeletionApproval = deletionApproved;
                await _pluginRepository.SavePlugin(plugin);
                TempData["StatusMessage"] = string.Format("Success! Plugin deletion request was {0}!", deletionApproved ? "sent" : "rejected");
                return Content("");
            }

            await _pluginRepository.RemovePlugin(id);
            await _commentsRepository.DeleteComments(id);
            await _loggingRepository.Log(User, id, $"<b>{User.Identity.Name}</b> removed <b>{plugin.Name}</b> at {DateTime.Now}");
            TempData["StatusMessage"] = "Success! Plugin was removed!";
            return Content("");
        }

        [HttpPost]
        public async Task<IActionResult> Save(PluginDetails plugin, Status status, bool isEditMode, bool saveStatusOnly)
        {
            try
            {
                plugin.Status = status;
                plugin.HasAdminConsent = plugin.HasAdminConsent || status == Status.InReview;
                plugin.WasActive = plugin.WasActive || status == Status.Active;

                if (isEditMode)
                {
                    var oldPlugin = await _pluginRepository.GetPluginById(plugin.Id, User);
                    var oldPluginToBase = PluginDetailsBase<PluginVersionBase<string>, string>.CopyFrom(plugin);
                    var newPluginToBase = PluginDetailsBase<PluginVersionBase<string>, string>.CopyFrom(oldPlugin);

                    if (!oldPluginToBase.Equals(newPluginToBase))
                    {
                        await _loggingRepository.Log(User, oldPluginToBase, newPluginToBase);
                    }
                    
                    await _pluginRepository.SavePlugin(plugin);

                    if (!string.IsNullOrEmpty(plugin.DownloadUrl) && !saveStatusOnly)
                    {
                        var response = await PluginPackage.DownloadPlugin(plugin.DownloadUrl);
                        (TempData["IsNameMatch"], TempData["IsAuthorMatch"]) = response.CreatePluginMatchLog(plugin, out bool isFullMatch);

                        if (!isFullMatch)
                        {
                            TempData["StatusMessage"] = "Warning! Plugin was saved but there are manifest conflicts!";
                            return new EmptyResult();
                        }
                    }
                }
                else
                {
                    await _pluginRepository.SavePlugin(plugin);
                    await _loggingRepository.Log(User, plugin.Id, $"<b>{User.Identity.Name}</b> added <b>{plugin.Name}</b> at {DateTime.Now}");
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
            const string searchFilter = "Query";
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
