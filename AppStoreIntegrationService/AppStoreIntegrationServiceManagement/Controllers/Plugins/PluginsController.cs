using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceManagement.Filters;
using AppStoreIntegrationServiceManagement.Model;
using AppStoreIntegrationServiceManagement.Model.DataBase;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using AppStoreIntegrationServiceManagement.Repository;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppStoreIntegrationServiceManagement.Controllers.Plugins
{
    [Area("Plugins")]
    [Authorize]
    [AccountSelected]
    public class PluginsController : CustomController
    {
        private readonly IPluginRepository _pluginRepository;
        private readonly IProductsRepository _productsRepository;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly ICommentsRepository _commentsRepository;
        private readonly ILoggingRepository _loggingRepository;
        private readonly INotificationCenter _notificationCenter;
        private readonly UserManager<IdentityUserExtended> _userManager;
        private readonly AccountsManager _accountsManager;
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
            INotificationCenter notificationCenter,
            UserManager<IdentityUserExtended> userManager,
            AccountsManager accountsManager
        )
        {
            _pluginRepository = pluginRepository;
            _productsRepository = productsRepository;
            _categoriesRepository = categoriesRepository;
            _commentsRepository = commentsRepository;
            _loggingRepository = loggingRepository;
            _notificationCenter = notificationCenter;
            _userManager = userManager;
            _accountsManager = accountsManager;
            _host = context.HttpContext.Request.Host.Value;
            _scheme = context.HttpContext.Request.Scheme;
        }

        [Route("Plugins")]
        [Route("/")]
        public async Task<IActionResult> Index()
        {
            PluginFilter filter = ApplyFilters();
            var plugins = await _pluginRepository.GetAll(filter.SortOrder, User.Identity.Name, ExtendedUser.Role);
            var products = await _productsRepository.GetAllProducts();
            var parents = await _productsRepository.GetAllParents();
            var status = new[] { "Active", "Inactive", "Draft", "InReview" }.Select(x => new FilterItem
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
                Developer = new DeveloperDetails { DeveloperName = User.Identity.Name },
                IsEditMode = false,
                Status = ExtendedUser.IsInRole("Developer") ? Status.Draft : Status.Active,
                IsThirdParty = ExtendedUser.IsInRole("Developer"),
                CategoryListItems = new MultiSelectList(categories, nameof(CategoryDetails.Id), nameof(CategoryDetails.Name))
            });
        }

        [Route("Plugins/Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id) => await Render(id, Status.Active, Pending, "Details");

        [Route("Plugins/Pending/{id:int}")]
        [RoleAuthorize("Administrator", "Developer")]
        public async Task<IActionResult> Pending(int id) => await Render(id, Status.InReview, Draft, "Pending");

        [Route("Plugins/Draft/{id:int}")]
        [RoleAuthorize("Administrator", "Developer")]
        public async Task<IActionResult> Draft(int id) => await Render(id, Status.Draft, Edit, "Draft");

        [HttpPost]
        [RoleAuthorize("Developer")]
        public async Task<IActionResult> RequestDeletion(int id)
        {
            var plugin = await _pluginRepository.GetPluginById(id, User.Identity.Name, ExtendedUser.Role);
            var log = string.Format(TemplateResource.DeletionRequestLog, User.Identity.Name, plugin.Name, DateTime.Now);

            if (!plugin.IsActive)
            {
                return await Delete(id);
            }

            plugin.NeedsDeletionApproval = true;
            await _pluginRepository.SavePlugin(plugin);
            await _loggingRepository.Log(User.Identity.Name, id, log);
            await Notify(NotificationTemplate.PluginDeletionRequest, plugin);
            TempData["StatusMessage"] = "Success! Plugin deletion request was sent!";
            return new EmptyResult();
        }

        [HttpPost]
        [RoleAuthorize("Administrator")]
        [Owner]
        public async Task<IActionResult> AcceptDeletion(int id)
        {
            var plugin = await _pluginRepository.GetPluginById(id);
            var log = string.Format(TemplateResource.DeletionAcceptedLog, User.Identity.Name, plugin.Name, DateTime.Now);
            await Notify(NotificationTemplate.PluginDeletionApproved, plugin);
            await _loggingRepository.Log(User.Identity.Name, id, log);
            return await Delete(id);
        }

        [HttpPost]
        [RoleAuthorize("Administrator")]
        [Owner]
        public async Task<IActionResult> RejectDeletion(int id)
        {
            var plugin = await _pluginRepository.GetPluginById(id);
            var log = string.Format(TemplateResource.DeletionRejectedLog, User.Identity.Name, plugin.Name, DateTime.Now);
            plugin.NeedsDeletionApproval = false;
            await _pluginRepository.SavePlugin(plugin);
            await _loggingRepository.Log(User.Identity.Name, id, log);
            await Notify(NotificationTemplate.PluginDeletionRejected, plugin);
            TempData["StatusMessage"] = "Success! Plugin deletion request was rejected!";
            return new EmptyResult();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var plugin = await _pluginRepository.GetPluginById(id, username: User.Identity.Name, userRole: ExtendedUser.Role);
            var log = string.Format(TemplateResource.PluginRemovedLog, User.Identity.Name, plugin.Name, DateTime.Now);
            await _pluginRepository.RemovePlugin(id);
            await _commentsRepository.DeleteComments(id);
            await _loggingRepository.Log(User.Identity.Name, id, log);
            TempData["StatusMessage"] = "Success! Plugin was removed!";
            return new EmptyResult();
        }

        [HttpPost]
        [RoleAuthorize("Administrator")]
        [Owner]
        public async Task<IActionResult> Activate(PluginDetails plugin)
        {
            var oldPlugin = await _pluginRepository.GetPluginById(plugin.Id, status: plugin.Status);
            string log = string.Format(TemplateResource.PluginActiveLog, User.Identity.Name, plugin.Name, DateTime.Now);
            plugin.Status = Status.Active;
            return await Save(plugin, oldPlugin, "Edit", log);
        }

        [HttpPost]
        [RoleAuthorize("Administrator")]
        [Owner]
        public async Task<IActionResult> Deactivate(PluginDetails plugin)
        {
            var oldPlugin = await _pluginRepository.GetPluginById(plugin.Id, status: plugin.Status);
            string log = string.Format(TemplateResource.PluginInactiveLog, User.Identity.Name, plugin.Name, DateTime.Now);
            plugin.Status = Status.Inactive;
            return await Save(plugin, oldPlugin, "Edit", log);
        }

        [HttpPost]
        [RoleAuthorize("Developer")]
        public async Task<IActionResult> Submit(PluginDetails plugin, bool removeOtherVersions)
        {
            var oldPlugin = await _pluginRepository.GetPluginById(plugin.Id, status: plugin.Status);
            plugin.Status = Status.InReview;
            plugin.HasAdminConsent = true;
            await Notify(NotificationTemplate.PluginReviewRequest, plugin);
            return await Save(plugin, oldPlugin, "Pending", CreateChangesLog(plugin, oldPlugin), removeOtherVersions, true);
        }

        [HttpPost]
        [RoleAuthorize("Administrator")]
        [Owner]
        public async Task<IActionResult> Approve(PluginDetails plugin, bool removeOtherVersions = false)
        {
            var oldPlugin = await _pluginRepository.GetPluginById(plugin.Id, status: plugin.Status);
            var log = string.Format(TemplateResource.ApprovedPluginLog, User.Identity.Name, plugin.Name, DateTime.Now);
            plugin.Status = Status.Active;
            plugin.IsActive = true;
            await Notify(NotificationTemplate.PluginApprovedRequest, plugin);
            return await Save(plugin, oldPlugin, "Edit", log, removeOtherVersions, true);
        }

        [HttpPost]
        [RoleAuthorize("Administrator")]
        [Owner]
        public async Task<IActionResult> Reject(PluginDetails plugin, bool removeOtherVersions = false)
        {
            var oldPlugin = await _pluginRepository.GetPluginById(plugin.Id, status: plugin.Status);
            string log = string.Format(TemplateResource.RejectedPluginLog, User.Identity.Name, plugin.Name, DateTime.Now);
            plugin.Status = Status.Draft;
            plugin.HasAdminConsent = true;
            await Notify(NotificationTemplate.PluginRejectedRequest, plugin);
            return await Save(plugin, oldPlugin, "Draft", log, removeOtherVersions);
        }

        [RoleAuthorize("Developer")]
        public async Task<IActionResult> SaveAsDraft(PluginDetails plugin)
        {
            var oldPlugin = await _pluginRepository.GetPluginById(plugin.Id, status: plugin.Status);
            plugin.Status = Status.Draft;
            plugin.HasAdminConsent = false;
            return await Save(plugin, oldPlugin, "Draft", CreateChangesLog(plugin, oldPlugin));
        }

        [HttpPost]
        public async Task<IActionResult> Save(PluginDetails plugin)
        {
            var oldPlugin = await _pluginRepository.GetPluginById(plugin.Id, status: Status.Active);
            return await Save(plugin, oldPlugin, "Edit", CreateChangesLog(plugin, oldPlugin), compareWithManifest: true);
        }

        private async Task Notify(NotificationTemplate notificationTemplate, PluginDetails plugin)
        {
            var account = _accountsManager.GetAppStoreAccount();
            var emailNotification = _notificationCenter.GetNotification(notificationTemplate, true, plugin.Icon.MediaUrl, plugin.Name, plugin.Id);
            var pushNotification = _notificationCenter.GetNotification(notificationTemplate, false, plugin.Icon.MediaUrl, plugin.Name, plugin.Id);
            await _notificationCenter.SendEmail(emailNotification, plugin.Developer.DeveloperName);
            await _notificationCenter.Push(pushNotification, plugin.Developer.DeveloperName);
            await _notificationCenter.Broadcast(emailNotification);
            await _notificationCenter.Push(pushNotification, account.AccountName);
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
            var plugin = await _pluginRepository.GetPluginById(id, User.Identity.Name, ExtendedUser.Role, status);

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

        private string CreateChangesLog(PluginDetails latest, PluginDetails old)
        {
            var oldToBase = PluginDetailsBase<PluginVersionBase<string>, string>.CopyFrom(old);
            var newToBase = PluginDetailsBase<PluginVersionBase<string>, string>.CopyFrom(latest);

            if (oldToBase == null)
            {
                return string.Format(TemplateResource.NewPluginLog, User.Identity.Name, latest.Name, DateTime.Now, CreateNewLog(newToBase));
            }

            if (oldToBase.Equals(newToBase))
            {
                return null;
            }

            return string.Format(TemplateResource.PluginUpdateLog, User.Identity.Name, latest.Name, DateTime.Now, CreateComparisonLog(newToBase, oldToBase));
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

        private string CreateComparisonLog(PluginDetailsBase<PluginVersionBase<string>, string> latest, PluginDetailsBase<PluginVersionBase<string>, string> old)
        {
            string change = "<li>The property <b>{0}</b> changed from <i>{1}</i> to <i>{2}</i></li>";
            return (latest.Name == old.Name ? null : string.Format(change, "Plugin name", latest.Name, old.Name)) +
                   (latest.Status == old.Status ? null : string.Format(change, "Status", latest.Status, old.Status)) +
                   (latest.PaidFor == old.PaidFor ? null : string.Format(change, "Pricing", latest.PaidFor, old.PaidFor)) +
                   (latest.Icon.Equals(old.Icon) ? null : string.Format(change, "Icon URL", latest.Icon.MediaUrl, old.Icon.MediaUrl)) +
                   (latest.SupportUrl == old.SupportUrl ? null : string.Format(change, "Support URL", latest.SupportUrl, old.SupportUrl)) +
                   (latest.Description == old.Description ? null : string.Format(change, "Description", latest.Description, old.Description)) +
                   (latest.SupportEmail == old.SupportEmail ? null : string.Format(change, "Support e-mail", latest.SupportEmail, old.SupportEmail)) +
                   (latest.ChangelogLink == old.ChangelogLink ? null : string.Format(change, "Changelog link", latest.ChangelogLink, old.ChangelogLink)) +
                   (latest.Developer.Equals(old.Developer) ? null : string.Format(change, "Developer", latest.Developer.DeveloperName, old.Developer.DeveloperName)) +
                   (latest.Categories.SequenceEqual(old.Categories) ? null : string.Format(change, "Categories", CreateCategoriesLog(latest.Categories), CreateCategoriesLog(@old.Categories)));
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
