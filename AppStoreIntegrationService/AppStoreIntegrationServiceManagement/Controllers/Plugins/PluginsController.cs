using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceManagement.Filters;
using AppStoreIntegrationServiceManagement.Model;
using AppStoreIntegrationServiceManagement.Model.DataBase;
using AppStoreIntegrationServiceManagement.Model.Notifications;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
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

        public PluginsController
        (
            IPluginRepository pluginRepository,
            IProductsRepository productsRepository,
            ICategoriesRepository categoriesRepository,
            ICommentsRepository commentsRepository,
            ILoggingRepository loggingRepository,
            INotificationCenter notificationCenter
        )
        {
            _pluginRepository = pluginRepository;
            _productsRepository = productsRepository;
            _categoriesRepository = categoriesRepository;
            _commentsRepository = commentsRepository;
            _loggingRepository = loggingRepository;
            _notificationCenter = notificationCenter;
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
                Icon = new IconDetails { MediaUrl = $"{GetUrlBase}/images/plugin.ico" },
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

            var notification = new EmailNotification(plugin)
            {
                CallToActionUrl = $"{GetUrlBase()}/Plugins?Query={plugin.Name}",
                Message = "There is a new deletion request awaiting approval!"
            };

            await Notify(notification, new PushNotification(notification));
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

            var notification = new EmailNotification(plugin)
            {
                CallToActionUrl = $"{GetUrlBase()}/Plugins?Query={plugin.Name}",
                Message = "Plugin deletion request was approved!"
            };

            await Notify(notification, new PushNotification(notification));
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

            var notification = new EmailNotification(plugin)
            {
                CallToActionUrl = $"{GetUrlBase()}/Plugins?Query={plugin.Name}",
                Message = "Plugin deletion request was rejected!"
            };

            await Notify(notification, new PushNotification(notification));
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

            var notification = new EmailNotification(plugin)
            {
                CallToActionUrl = $"{GetUrlBase()}/Plugins/Pending/{plugin.Id}",
                Message = "A new plugin was submitted for approval!"
            };

            await Notify(notification, new PushNotification(notification));
            var log = _loggingRepository.CreateChangesLog(plugin, oldPlugin, User.Identity.Name);
            return await Save(plugin, oldPlugin, "Pending", log, removeOtherVersions, true);
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

            var notification = new EmailNotification(plugin)
            {
                CallToActionUrl = $"{GetUrlBase()}/Plugins/Edit/{plugin.Id}",
                Message = "A submited plugin was approved!"
            };

            await Notify(notification, new PushNotification(notification));
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

            var notification = new EmailNotification(plugin)
            {
                CallToActionUrl = $"{GetUrlBase()}/Plugins/Draft/{plugin.Id}",
                Message = "A submited plugin was rejected!"
            };

            await Notify(notification, new PushNotification(notification));
            return await Save(plugin, oldPlugin, "Draft", log, removeOtherVersions);
        }

        [RoleAuthorize("Developer")]
        public async Task<IActionResult> SaveAsDraft(PluginDetails plugin)
        {
            var oldPlugin = await _pluginRepository.GetPluginById(plugin.Id, status: plugin.Status);
            plugin.Status = Status.Draft;
            plugin.HasAdminConsent = false;
            return await Save(plugin, oldPlugin, "Draft", _loggingRepository.CreateChangesLog(plugin, oldPlugin, User.Identity.Name));
        }

        [HttpPost]
        public async Task<IActionResult> Save(PluginDetails plugin)
        {
            var oldPlugin = await _pluginRepository.GetPluginById(plugin.Id, status: Status.Active);
            var log = _loggingRepository.CreateChangesLog(plugin, oldPlugin, User.Identity.Name);
            return await Save(plugin, oldPlugin, "Edit", log, compareWithManifest: true);
        }

        private async Task Notify(EmailNotification emailNotification, PushNotification pushNotification)
        {
            await _notificationCenter.SendEmail(emailNotification);
            await _notificationCenter.Push(pushNotification);
            await _notificationCenter.Broadcast(emailNotification);
            pushNotification.Author = AccountsManager.GetAppStoreAccount().AccountName;
            await _notificationCenter.Push(pushNotification);
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
    }
}
