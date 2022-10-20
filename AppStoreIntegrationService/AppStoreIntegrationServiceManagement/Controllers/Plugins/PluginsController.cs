using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.V2.Interface;
using AppStoreIntegrationServiceManagement.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceManagement.Controllers.Plugins
{
    [Authorize]
    [Area("Plugins")]
    public class PluginsController : Controller
    {
        private readonly IPluginRepositoryExtended<PluginDetails<PluginVersion<string>, string>> _pluginRepositoryExtended;
        private readonly IProductsRepository _productsRepository;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IHttpContextAccessor _context;

        public PluginsController
        (
            IPluginRepositoryExtended<PluginDetails<PluginVersion<string>, string>> pluginRepositoryExtended,
            IHttpContextAccessor context,
            IProductsRepository productsRepository,
            ICategoriesRepository categoriesRepository
        )
        {
            _pluginRepositoryExtended = pluginRepositoryExtended;
            _context = context;
            _productsRepository = productsRepository;
            _categoriesRepository = categoriesRepository;
        }

        [Route("Plugins")]
        [Route("/")]
        public async Task<IActionResult> Index()
        {
            PluginFilter pluginsFilters = ApplyFilters();
            var pluginsList = await _pluginRepositoryExtended.GetAll(pluginsFilters.SortOrder);
            _pluginRepositoryExtended.SearchPlugins(pluginsList, pluginsFilters);
            return View(InitializePrivatePlugins(_pluginRepositoryExtended.SearchPlugins(pluginsList, pluginsFilters)).ToList());
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
            return await Save(plugin, versions, version, _pluginRepositoryExtended.AddPrivatePlugin);
        }

        [Route("Plugins/Edit/{id?}")]
        public async Task<IActionResult> Edit(int id)
        {
            var categories = await _categoriesRepository.GetAllCategories();
            var pluginDetails = await _pluginRepositoryExtended.GetPluginById(id);
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
            return await Save(plugin, versions, version, _pluginRepositoryExtended.UpdatePrivatePlugin);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _pluginRepositoryExtended.RemovePlugin(id);
            TempData["StatusMessage"] = "Success! Plugin was removed!";
            return Content("");
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
            var foundPluginDetails = await _pluginRepositoryExtended.GetPluginById(plugin.Id);
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
            var query = Request.Query;
            var filters = new PluginFilter()
            {
                SortOrder = "asc",
                Status = PluginFilter.StatusValue.All,
                Query = query.ContainsKey(searchFilter) ? query[searchFilter] : default
            };

            if (query.ContainsKey(statusFilter))
            {
                bool isValidType = int.TryParse(query[statusFilter], out int statusValueIndex);
                if (isValidType && Enum.IsDefined(typeof(PluginFilter.StatusValue), statusValueIndex))
                {
                    filters.Status = (PluginFilter.StatusValue)statusValueIndex;
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
