using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
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
            var plugins = await _pluginRepository.GetAll("asc");
            var request = _context.HttpContext?.Request;

            return View("Details", new ExtendedPluginDetails
            {
                Id = plugins.MaxBy(x => x.Id)?.Id + 1 ?? 0,
                Icon = new IconDetails { MediaUrl = $"{request?.Scheme}://{request?.Host.Value}/images/plugin.ico" },
                Developer = new DeveloperDetails { DeveloperName = User.IsInRole("Developer") ? User.Identity.Name : null },
                IsEditMode = false,
                Status = User.IsInRole("Developer") ? Status.Draft : Status.Active,
                IsThirdParty = User.IsInRole("Developer"),
                CategoryListItems = new MultiSelectList(categories, nameof(CategoryDetails.Id), nameof(CategoryDetails.Name))
            });
        }

        [Route("Plugins/Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var plugin = await _pluginRepository.GetPluginById(id, Status.Active);
            var view = User.IsInRole("StandardUser") && plugin.IsThirdParty ? "ReadonlyDetails" : "Details";
            return await Render(id, Status.Active, _pluginRepository.GetPluginById, "Pending", view);
        }

        [Route("Plugins/Pending/{id:int}")]
        [Authorize(Roles = "Administrator, Developer")]
        public async Task<IActionResult> Pending(int id)
        {
            return await Render(id, Status.InReview, _pluginRepository.GetPluginById, "Draft", "Pending");
        }

        [Route("Plugins/Draft/{id:int}")]
        [Authorize(Roles = "Administrator, Developer")]
        public async Task<IActionResult> Draft(int id)
        {
            return await Render(id, Status.Draft, _pluginRepository.GetPluginById, "Edit", "Draft");
        }

        [HttpPost]
        [Authorize(Roles = "Developer")]
        public async Task<IActionResult> RequestDeletion(int id)
        {
            var plugin = await _pluginRepository.GetPluginById(id, Status.All, User);
            if (plugin.IsActive)
            {
                plugin.NeedsDeletionApproval = true;
                await _pluginRepository.SavePlugin(plugin);
                TempData["StatusMessage"] = "Success! Plugin deletion request was sent!";
                return Content(null);
            }

            return RedirectToAction("Delete", new { id });
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RejectDeletion(int id)
        {
            var plugin = await _pluginRepository.GetPluginById(id, Status.All, User);
            plugin.NeedsDeletionApproval = false;
            await _pluginRepository.SavePlugin(plugin);
            await _loggingRepository.Log(User, id, $"<b>{User.Identity.Name}</b> rejected deletion for <b>{plugin.Name}</b> at {DateTime.Now}");
            TempData["StatusMessage"] = "Success! Plugin deletion request was rejected!";
            return Content(null);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var plugin = await _pluginRepository.GetPluginById(id, Status.All, User);
            await _pluginRepository.RemovePlugin(id);
            await _commentsRepository.DeleteComments(id);
            await _loggingRepository.Log(User, id, $"<b>{User.Identity.Name}</b> removed <b>{plugin.Name}</b> at {DateTime.Now}");
            TempData["StatusMessage"] = "Success! Plugin was removed!";
            return Content("");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(PluginDetails plugin, Status status)
        {
            plugin.Status = status;
            var oldPlugin = await _pluginRepository.GetPluginById(plugin.Id, Status.Active);
            await _loggingRepository.Log(User, plugin.Id, $"<b>{User.Identity.Name}</b> changed the status of <b>{plugin.Name}</b> from <b>{oldPlugin.Status}</b> to <b>{plugin.Status}</b>");
            await _pluginRepository.SavePlugin(plugin, false);
            TempData["StatusMessage"] = $"Success! {plugin.Name} was saved!";

            return Content($"/Plugins/Edit/{plugin.Id}");
        }

        [HttpPost]
        [Authorize(Roles = "Developer")]
        public async Task<IActionResult> Submit(PluginDetails plugin, bool removeOtherVersions)
        {
            plugin.Status = Status.InReview;
            plugin.HasAdminConsent = true;
            string log = "<b>{0}</b> submitted new changes for <b>{1}</b> at {2}";
            var oldPlugin = await _pluginRepository.GetPluginById(plugin.Id, Status.InReview);
            return await SaveChanges(plugin, oldPlugin, log, "Pending", removeOtherVersions);
        }

        [HttpPost]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> Approve(PluginDetails plugin, bool removeOtherVersions = false)
        {
            plugin.Status = Status.Active;
            plugin.IsActive = true;
            string log = "<b>{0}</b> accepted the changes for <b>{1}</b> at {2}";
            var oldPlugin = await _pluginRepository.GetPluginById(plugin.Id, Status.Active);
            return await SaveChanges(plugin, oldPlugin, log, "Edit", removeOtherVersions);
        }

        [HttpPost]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> Reject(PluginDetails plugin, bool removeOtherVersions = false)
        {
            plugin.Status = Status.Draft;
            plugin.HasAdminConsent = true;
            string log = "<b>{0}</b> rejected the changes for <b>{1}</b> at {2}";
            var oldPlugin = await _pluginRepository.GetPluginById(plugin.Id, Status.InReview);
            return await SaveChanges(plugin, oldPlugin, log, "Draft", removeOtherVersions);
        }

        [HttpPost]
        [Authorize(Roles = "Developer")]
        public async Task<IActionResult> SaveAsDraft(PluginDetails plugin)
        {
            plugin.Status = Status.Draft;
            plugin.HasAdminConsent = false;
            string log = "<b>{0}</b> drafted a new version for <b>{1}</b> at {2}";
            var oldPlugin = await _pluginRepository.GetPluginById(plugin.Id, Status.Draft);
            return await SaveChanges(plugin, oldPlugin, log, "Draft");
        }

        [HttpPost]
        public async Task<IActionResult> Save(PluginDetails plugin)
        {
            string log = "<b>{0}</b> accepted the changes for <b>{1}</b> at {2}";
            var oldPlugin = await _pluginRepository.GetPluginById(plugin.Id, Status.Active);
            return await SaveChanges(plugin, oldPlugin, log, "Edit");
        }

        private async Task<IActionResult> SaveChanges(PluginDetails plugin, PluginDetails oldPlugin, string log, string successRedirect, bool removeOtherVersions = false)
        {
            try
            {
                if (oldPlugin == null)
                {
                    await _loggingRepository.Log(User, plugin.Id, string.Format(log, User.Identity.Name, plugin.Name, DateTime.Now));
                    await _pluginRepository.SavePlugin(plugin, removeOtherVersions);
                    TempData["StatusMessage"] = $"Success! {plugin.Name} was saved!";
                    return Content($"/Plugins/{successRedirect}/{plugin.Id}");
                }

                var oldPluginToBase = PluginDetailsBase<PluginVersionBase<string>, string>.CopyFrom(plugin);
                var newPluginToBase = PluginDetailsBase<PluginVersionBase<string>, string>.CopyFrom(oldPlugin);
                TempData["StatusMessage"] = $"Success! Nothing to save, {plugin.Name} is up to date!";

                if (!oldPluginToBase.Equals(newPluginToBase))
                {
                    await _loggingRepository.Log(User, oldPluginToBase, newPluginToBase);
                    await _pluginRepository.SavePlugin(plugin, removeOtherVersions);
                    TempData["StatusMessage"] = $"Success! {plugin.Name} was saved!";
                    await CompareWithManifest(plugin);
                }

                return Content($"/Plugins/{successRedirect}/{plugin.Id}");
            }
            catch (Exception e)
            {
                return PartialView("_StatusMessage", string.Format("Error! {0}!", e.Message));
            }
        }

        private async Task CompareWithManifest(PluginDetails plugin)
        {
            if (string.IsNullOrEmpty(plugin.DownloadUrl))
            {
                return;
            }

            var response = await PluginPackage.DownloadPlugin(plugin.DownloadUrl);
            (TempData["IsNameMatch"], TempData["IsAuthorMatch"]) = response.CreatePluginMatchLog(plugin, out bool isFullMatch);

            if (isFullMatch)
            {
                return;
            }

            TempData["StatusMessage"] = "Warning! Plugin was saved but there are manifest conflicts!";
        }

        private async Task<IActionResult> Render(int id, Status status, Func<int, Status, ClaimsPrincipal, Task<PluginDetails>> getPluginById, string viewIfNull, string viewIfSuccess)
        {
            if (!await _pluginRepository.ExitsPlugin(id))
            {
                return NotFound();
            }

            var categories = await _categoriesRepository.GetAllCategories();
            var plugin = await getPluginById(id, status, User);

            if (plugin == null)
            {
                return Redirect($"/Plugins/{viewIfNull}/{id}");
            }

            var extended = ExtendedPluginDetails.CopyFrom(plugin);
            extended.CategoryListItems = new MultiSelectList(categories, nameof(CategoryDetails.Id), nameof(CategoryDetails.Name));
            extended.IsEditMode = true;

            return View(viewIfSuccess, extended);
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
    }
}
