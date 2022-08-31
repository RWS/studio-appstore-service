using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model.Plugins;
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
        private readonly IPluginRepository _pluginRepository;
        private readonly IHttpContextAccessor _context;

        public PluginsController(IPluginRepository pluginRepository, IHttpContextAccessor context)
        {
            _pluginRepository = pluginRepository;
            _context = context;
        }

        [Route("Plugins")]
        public async Task<IActionResult> Index()
        {
            PluginFilter pluginsFilters = ApplyFilters();
            List<PluginDetails> pluginsList = await _pluginRepository.GetAll(pluginsFilters.SortOrder);
            _pluginRepository.SearchPlugins(pluginsList, pluginsFilters);
            return View(InitializePrivatePlugins(_pluginRepository.SearchPlugins(pluginsList, pluginsFilters)).ToList());
        }

        [Route("Plugins/New")]
        public async Task<IActionResult> New()
        {
            var categories = await _pluginRepository.GetCategories();
            return View(new PluginDetailsModel
            {
                PrivatePlugin = new PrivatePlugin
                {
                    IconUrl = GetDefaultIcon(),
                    IsEditMode = false
                },
                SelectedVersionId = Guid.NewGuid().ToString(),
                Categories = categories,
                CategoryListItems = new SelectList(categories, nameof(CategoryDetails.Id), nameof(CategoryDetails.Name))
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(PluginDetailsModel pluginDetails, List<PluginVersion> versions, PluginVersion version)
        {
            return await Save(pluginDetails, versions, version, _pluginRepository.AddPrivatePlugin);
        }

        [Route("Plugins/Edit/{id?}")]
        public async Task<IActionResult> Edit(int id)
        {
            var categories = await _pluginRepository.GetCategories();
            var pluginDetails = await _pluginRepository.GetPluginById(id);
            return View(new PluginDetailsModel
            {
                PrivatePlugin = new PrivatePlugin
                {
                    Id = pluginDetails.Id,
                    PaidFor = pluginDetails.PaidFor,
                    DeveloperName = pluginDetails.Developer?.DeveloperName,
                    Description = pluginDetails.Description,
                    Name = pluginDetails.Name,
                    Categories = pluginDetails.Categories,
                    Versions = SetSelectedProducts(pluginDetails.Versions, string.Empty).ToList(),
                    IconUrl = string.IsNullOrEmpty(pluginDetails.Icon.MediaUrl) ? GetDefaultIcon() : pluginDetails.Icon.MediaUrl,
                    IsEditMode = true
                },
                Categories = categories,
                CategoryListItems = new SelectList(categories, nameof(CategoryDetails.Id), nameof(CategoryDetails.Name))
            });
        }

        [HttpPost]
        public async Task<IActionResult> Update(PluginDetailsModel pluginDetails, List<PluginVersion> versions, PluginVersion version)
        {
            return await Save(pluginDetails, versions, version, _pluginRepository.UpdatePrivatePlugin);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _pluginRepository.RemovePlugin(id);
            return RedirectToAction("Index");
        }

        [Route("[controller]/[action]/{redirectUrl?}/{currentPage?}")]
        public async Task<IActionResult> GoToPage(PluginDetailsModel pluginDetails, PluginVersion version, string redirectUrl, string currentPage)
        {
            if (currentPage != "new" && await IsSaved(pluginDetails, version))
            {
                return Redirect(redirectUrl.Replace('.', '/'));
            }

            var modalDetails = new ModalMessage
            {
                RequestPage = $"{redirectUrl.Replace('.', '/')}",
                ModalType = ModalType.WarningMessage,
                Title = "Unsaved changes!",
                Message = $"Discard changes for {(string.IsNullOrEmpty(pluginDetails.PrivatePlugin.Name) ? "plugin" : pluginDetails.PrivatePlugin.Name)}?"
            };

            return PartialView("_ModalPartial", modalDetails);
        }

        private async Task<bool> IsSaved(PluginDetailsModel pluginDetails, PluginVersion version)
        {
            var plugin = pluginDetails.PrivatePlugin;
            var foundPluginDetails = await _pluginRepository.GetPluginById(plugin.Id);
            plugin.SetCategoryList(pluginDetails.SelectedCategories, await _pluginRepository.GetCategories());
            var newPluginDetails = plugin.ConvertToPluginDetails(foundPluginDetails, version);
            return JsonConvert.SerializeObject(newPluginDetails) == JsonConvert.SerializeObject(foundPluginDetails);
        }

        private async Task<IActionResult> Save(PluginDetailsModel pluginDetails, List<PluginVersion> versions, PluginVersion version, Func<PrivatePlugin, Task> func)
        {
            var plugin = pluginDetails.PrivatePlugin;
            var modalDetails = new ModalMessage()
            {
                RequestPage = plugin.IsEditMode ? "" : "add",
                Message = "Please fill all required values.",
                ModalType = ModalType.WarningMessage
            };
            
            if (plugin.IsValid(version))
            {
                plugin.SetVersionList(versions, version);
                plugin.SetCategoryList(pluginDetails.SelectedCategories, pluginDetails.Categories);
                plugin.SetDownloadUrl();

                try
                {
                    await func(plugin);
                    modalDetails.ModalType = ModalType.SuccessMessage;
                    modalDetails.Message = $"{plugin.Name} was updated.";
                    modalDetails.Id = plugin.Id;
                }
                catch (Exception e)
                {
                    modalDetails.Message = e.Message;
                }
            }

            return PartialView("_ModalPartial", modalDetails);
        }

        private static IEnumerable<PluginVersion> SetSelectedProducts(List<PluginVersion> versions, string versionName)
        {
            foreach (var version in versions)
            {
                var lastSupportedProduct = version.SupportedProducts.Last();
                version.SelectedProductId = lastSupportedProduct.Id;
                version.SelectedProduct = lastSupportedProduct;
                version.VersionName = version.IsNewVersion ? versionName : $"{version.SelectedProduct.ProductName} - {version.VersionNumber}";
                version.IsNewVersion = false;
                yield return version;
            }
        }

        private IEnumerable<PrivatePlugin> InitializePrivatePlugins(List<PluginDetails> plugins)
        {
            foreach (var plugin in plugins)
            {
                yield return new PrivatePlugin
                {
                    Id = plugin.Id,
                    Description = plugin.Description,
                    Name = plugin.Name,
                    Categories = plugin.Categories,
                    Versions = plugin.Versions,
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
