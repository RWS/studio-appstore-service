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
        private readonly NotificationCenter _notificationCenter;
        private readonly string _host;
        private readonly string _scheme;

        public PluginsController
        (
            IPluginRepository pluginRepository,
            IHttpContextAccessor context,
            IProductsRepository productsRepository,
            ICategoriesRepository categoriesRepository,
            ICommentsRepository commentsRepository,
            ILoggingRepository loggingRepository,
            NotificationCenter notificationCenter
        )
        {
            _pluginRepository = pluginRepository;
            _productsRepository = productsRepository;
            _categoriesRepository = categoriesRepository;
            _commentsRepository = commentsRepository;
            _loggingRepository = loggingRepository;
            _notificationCenter = notificationCenter;
            _host = context.HttpContext.Request.Host.Value;
            _scheme = context.HttpContext.Request.Scheme;
        }

        [Route("Plugins")]
        [Route("/")]
        public async Task<IActionResult> Index()
        {
            PluginFilter filter = ApplyFilters();
            var plugins = await _pluginRepository.GetAll(filter.SortOrder, User);
            var products = await _productsRepository.GetAllProducts();
            var parents = await _productsRepository.GetAllParents();
            var status = new List<string> { "Active", "Inactive" }.Concat(User.IsInRole("StandardUser") ? new List<string>() : new[] { "Draft", "InReview" }).Select(x => new FilterItem
            {
                Id = "Status",
                Label = x,
                Value = $"{(int)Enum.Parse(typeof(Status), x)}",
                IsSelected = Request.Query["Status"].Any(y => y == $"{(int)Enum.Parse(typeof(Status), x)}")
            });

            return View(new ConfigToolModel
            {
                Plugins = PluginFilter.FilterPlugins(plugins, filter, products, parents).Select(p => ExtendedPluginDetails.CopyFrom(p)),
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

            return View("Details", new ExtendedPluginDetails
            {
                Id = plugins.MaxBy(x => x.Id)?.Id + 1 ?? 0,
                Icon = new IconDetails { MediaUrl = $"{_scheme}://{_host}/images/plugin.ico" },
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
            return await Render(id, Status.Active, Pending, view);
        }

        [Route("Plugins/Pending/{id:int}")]
        [Authorize(Roles = "Administrator, Developer")]
        public async Task<IActionResult> Pending(int id) => await Render(id, Status.InReview, Draft, "Pending");

        [Route("Plugins/Draft/{id:int}")]
        [Authorize(Roles = "Administrator, Developer")]
        public async Task<IActionResult> Draft(int id) => await Render(id, Status.Draft, Edit, "Draft");

        [HttpPost]
        [Authorize(Roles = "Developer")]
        public async Task<IActionResult> RequestDeletion(int id)
        {
            var plugin = await _pluginRepository.GetPluginById(id, Status.All, User);
            if (plugin.IsActive)
            {
                plugin.NeedsDeletionApproval = true;
                _notificationCenter.SendEmail(_notificationCenter.GetDeletionRequestNotification(plugin.Icon.MediaUrl, plugin.Name), "New deletion request");
                await _pluginRepository.SavePlugin(plugin);
                await _loggingRepository.Log(User.Identity.Name, id, $"<b>{User.Identity.Name}</b> requested deletion for <b>{plugin.Name}</b> at {DateTime.Now}");
                TempData["StatusMessage"] = "Success! Plugin deletion request was sent!";
                return Content(null);
            }

            return await Delete(id);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AcceptDeletion(int id)
        {
            var plugin = await _pluginRepository.GetPluginById(id, Status.All, User);
            _notificationCenter.SendEmail(_notificationCenter.GetDeletionApprovedNotification(plugin.Icon.MediaUrl, plugin.Name), "Deletion request approved");
            await _loggingRepository.Log(User.Identity.Name, id, $"<b>{User.Identity.Name}</b> accepted deletion for <b>{plugin.Name}</b> at {DateTime.Now}");
            return await Delete(id);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RejectDeletion(int id)
        {
            var plugin = await _pluginRepository.GetPluginById(id, Status.All, User);
            plugin.NeedsDeletionApproval = false;
            _notificationCenter.SendEmail(_notificationCenter.GetDeletionRejectedNotification(plugin.Icon.MediaUrl, plugin.Name), "Deletion request rejected");
            await _pluginRepository.SavePlugin(plugin);
            await _loggingRepository.Log(User.Identity.Name, id, $"<b>{User.Identity.Name}</b> rejected deletion for <b>{plugin.Name}</b> at {DateTime.Now}");
            TempData["StatusMessage"] = "Success! Plugin deletion request was rejected!";
            return Content(null);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var plugin = await _pluginRepository.GetPluginById(id, Status.All, User);
            await _pluginRepository.RemovePlugin(id);
            await _commentsRepository.DeleteComments(id);
            await _loggingRepository.Log(User.Identity.Name, id, $"<b>{User.Identity.Name}</b> removed <b>{plugin.Name}</b> at {DateTime.Now}");
            TempData["StatusMessage"] = "Success! Plugin was removed!";
            return Content(null);
        }

        [HttpPost]
        public async Task<IActionResult> Activate(PluginDetails plugin)
        {
            var oldPlugin = await _pluginRepository.GetPluginById(plugin.Id, plugin.Status);
            plugin.Status = Status.Active;
            string log = $"<b>{User.Identity.Name} </b> changed the status to <i>Active</i> for <b> {plugin.Name} </b> at  {DateTime.Now}";
            return await Save(plugin, oldPlugin, "Edit", log);
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(PluginDetails plugin)
        {
            var oldPlugin = await _pluginRepository.GetPluginById(plugin.Id, plugin.Status);
            plugin.Status = Status.Inactive;
            string log = $"<b>{User.Identity.Name}</b> changed the status to <i>Inactive</i> for <b>{plugin.Name}</b> at {DateTime.Now}";
            return await Save(plugin, oldPlugin, "Edit", log);
        }

        [HttpPost]
        [Authorize(Roles = "Developer")]
        public async Task<IActionResult> Submit(PluginDetails plugin, bool removeOtherVersions)
        {
            var oldPlugin = await _pluginRepository.GetPluginById(plugin.Id, plugin.Status);
            plugin.Status = Status.InReview;
            plugin.HasAdminConsent = true;
            _notificationCenter.SendEmail(_notificationCenter.GetReviewRequestNotification(plugin.Icon.MediaUrl, plugin.Name, plugin.Id), "New plugin sent to review");
            return await Save(plugin, oldPlugin, "Pending", CreateChangesLog(plugin, oldPlugin), removeOtherVersions, true);
        }

        [HttpPost]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> Approve(PluginDetails plugin, bool removeOtherVersions = false)
        {
            var oldPlugin = await _pluginRepository.GetPluginById(plugin.Id, plugin.Status);
            plugin.Status = Status.Active;
            plugin.IsActive = true;
            _notificationCenter.SendEmail(_notificationCenter.GetApprovedNotification(plugin.Icon.MediaUrl, plugin.Name, plugin.Id), "Your plugin was approved");
            string log = $"<b>{User.Identity.Name}</b> accepted the changes for <b>{plugin.Name}</b> at {DateTime.Now}";
            return await Save(plugin, oldPlugin, "Edit", log, removeOtherVersions, true);
        }

        [HttpPost]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> Reject(PluginDetails plugin, bool removeOtherVersions = false)
        {
            var oldPlugin = await _pluginRepository.GetPluginById(plugin.Id, plugin.Status);
            plugin.Status = Status.Draft;
            plugin.HasAdminConsent = true;
            _notificationCenter.SendEmail(_notificationCenter.GetRejectedNotification(plugin.Icon.MediaUrl, plugin.Name, plugin.Id), "Your plugin was rejected");
            string log = $"<b>{User.Identity.Name}</b> rejected the changes for <b>{plugin.Name}</b> at {DateTime.Now}";
            return await Save(plugin, oldPlugin, "Draft", log, removeOtherVersions);
        }

        [HttpPost]
        [Authorize(Roles = "Developer")]
        public async Task<IActionResult> SaveAsDraft(PluginDetails plugin)
        {
            var oldPlugin = await _pluginRepository.GetPluginById(plugin.Id, plugin.Status);
            plugin.Status = Status.Draft;
            plugin.HasAdminConsent = false;
            return await Save(plugin, oldPlugin, "Draft", CreateChangesLog(plugin, oldPlugin));
        }

        [HttpPost]
        public async Task<IActionResult> Save(PluginDetails plugin)
        {
            var oldPlugin = await _pluginRepository.GetPluginById(plugin.Id, Status.Active);
            return await Save(plugin, oldPlugin, "Edit", CreateChangesLog(plugin, oldPlugin), compareWithManifest: true);
        }

        private async Task<IActionResult> Save(PluginDetails plugin, PluginDetails oldPlugin, string successRedirect, string log = null, bool removeOtherVersions = false, bool compareWithManifest = false)
        {
            try
            {
                if (!string.IsNullOrEmpty(log))
                {
                    await _loggingRepository.Log(User.Identity.Name, plugin.Id, log);
                }

                if (oldPlugin != null)
                {
                    plugin.Versions = oldPlugin.Versions;
                    plugin.Pending = oldPlugin.Pending;
                    plugin.Drafts = oldPlugin.Drafts;
                }

                await _pluginRepository.SavePlugin(plugin, removeOtherVersions);
                if (compareWithManifest)
                {
                    await CompareWithManifest(plugin);
                }

                TempData["StatusMessage"] = $"Success! {plugin.Name} was saved!";
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

        private async Task<IActionResult> Render(int id, Status status, Func<int, Task<IActionResult>> nullRedirect, string viewIfSuccess)
        {
            if (!await _pluginRepository.ExitsPlugin(id))
            {
                return NotFound();
            }

            var categories = await _categoriesRepository.GetAllCategories();
            var plugin = await _pluginRepository.GetPluginById(id, status, User);

            if (plugin == null)
            {
                return await nullRedirect(id);
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

        private string CreateChangesLog(PluginDetails @new, PluginDetails old)
        {
            var oldToBase = PluginDetailsBase<PluginVersionBase<string>, string>.CopyFrom(old);
            var newToBase = PluginDetailsBase<PluginVersionBase<string>, string>.CopyFrom(@new);

            if (oldToBase == null)
            {
                return $"<b>{User.Identity.Name}</b> added {@new.Name} at {DateTime.Now}<br><br><p>The plugin properties are:</p><ul>{CreateNewLog(newToBase)}</ul>";
            }

            if (oldToBase.Equals(newToBase))
            {
                return null;
            }

            return $"<b>{User.Identity.Name}</b> made changes to {@new.Name} at {DateTime.Now}<br><br><p>The following changes occured:</p><ul>{CreateComparisonLog(newToBase, oldToBase)}</ul>";
        }

        private string CreateNewLog(PluginDetailsBase<PluginVersionBase<string>, string> plugin)
        {
            string change = "<li><b>{0}</b> : <i>{1}</i></li>";
            return string.Format(change, "Plugin name", plugin.Name) +
                   string.Format(change, "Description", plugin.Description) +
                   string.Format(change, "Changelog link", plugin.ChangelogLink) +
                   string.Format(change, "Support URL", plugin.SupportUrl) +
                   string.Format(change, "Support e-mail", plugin.SupportEmail) +
                   string.Format(change, "Icon URL", plugin.Icon.MediaUrl) +
                   string.Format(change, "Pricing", plugin.PaidFor ? "Paid" : "Free") +
                   string.Format(change, "Developer", plugin.Developer.DeveloperName) +
                   string.Format(change, "Categories", CreateCategoriesLog(plugin.Categories)) +
                   string.Format(change, "Status", plugin.Status.ToString());
        }

        private string CreateComparisonLog(PluginDetailsBase<PluginVersionBase<string>, string> @new, PluginDetailsBase<PluginVersionBase<string>, string> old)
        {
            string change = "<li>The property <b>{0}</b> changed from <i>{1}</i> to <i>{2}</i></li>";
            return (@new.Name == old.Name ? null : string.Format(change, "Plugin name", @new.Name, old.Name)) +
                   (@new.Status == old.Status ? null : string.Format(change, "Status", @new.Status, old.Status)) +
                   (@new.PaidFor == old.PaidFor ? null : string.Format(change, "Pricing", @new.PaidFor, old.PaidFor)) +
                   (@new.Icon.Equals(old.Icon) ? null : string.Format(change, "Icon URL", @new.Icon.MediaUrl, old.Icon.MediaUrl)) +
                   (@new.SupportUrl == old.SupportUrl ? null : string.Format(change, "Support URL", @new.SupportUrl, old.SupportUrl)) +
                   (@new.Description == old.Description ? null : string.Format(change, "Description", @new.Description, old.Description)) +
                   (@new.SupportEmail == old.SupportEmail ? null : string.Format(change, "Support e-mail", @new.SupportEmail, old.SupportEmail)) +
                   (@new.ChangelogLink == old.ChangelogLink ? null : string.Format(change, "Changelog link", @new.ChangelogLink, old.ChangelogLink)) +
                   (@new.Developer.Equals(old.Developer) ? null : string.Format(change, "Developer", @new.Developer.DeveloperName, old.Developer.DeveloperName)) +
                   (@new.Categories.SequenceEqual(old.Categories) ? null : string.Format(change, "Categories", CreateCategoriesLog(@new.Categories), CreateCategoriesLog(@old.Categories)));
        }

        private string CreateCategoriesLog(List<string> categories)
        {
            var categoryDetails = _categoriesRepository.GetAllCategories().Result;
            if (categories.Count > 1)
            {
                return $"[{categories.Aggregate("", (result, next) => $"{result}, {categoryDetails.FirstOrDefault(c => c.Id == next).Name}")}]";
            }

            return $"[{categoryDetails.FirstOrDefault(c => c.Id == categories[0]).Name}]";
        }
    }
}
