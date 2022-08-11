using AppStoreIntegrationService.Controllers;
using AppStoreIntegrationService.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppStoreIntegrationService
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ConfigToolModel : PageModel
    {
        private readonly PluginsController _pluginsController;
        private readonly IHttpContextAccessor _context;

        [BindProperty]
        public List<PrivatePlugin> PrivatePlugins { get; set; }

        [BindProperty]
        public PrivatePlugin PrivatePlugin { get; set; }

        [BindProperty]
        public int Id { get; set; }
        [BindProperty]
        public string Name { get; set; }

        public ConfigToolModel(PluginsController pluginsController, IHttpContextAccessor context)
        {
            _pluginsController = pluginsController;
            _context = context;
        }

        public async Task OnGetAsync()
        {
            PrivatePlugins = new List<PrivatePlugin>();
            PluginFilter pluginsFilters = ApplyFilters();
            var privatePluginsResult = await _pluginsController.Get(pluginsFilters);
            if (privatePluginsResult is OkObjectResult resultObject && resultObject.StatusCode == 200)
            {
                if (resultObject.Value is List<PluginDetails> privatePlugins)
                {
                    InitializePrivatePlugins(privatePlugins);
                }
            }
        }

        public async Task<IActionResult> OnGetShowDeleteModal(int id,string name)
        {
            var privatePlugin = new PrivatePlugin
            {
                Id = id,
                Name = name
            };            

            return Partial("_DeletePluginPartial", privatePlugin);
        }

        public async Task<IActionResult> OnPostDeletePlugin()
        {
            await _pluginsController.DeletePlugin(Id);

            return RedirectToPage("ConfigTool");
        }

        public async Task<IActionResult> OnGetAddPlugin()
        {
            return Partial("_AddPluginPartial", new PrivatePlugin());            
        }

        private void InitializePrivatePlugins(List<PluginDetails> privatePlugins)
        {
            foreach (var pluginDetails in privatePlugins)
            {
                var privatePlugin = new PrivatePlugin
                {
                    Id = pluginDetails.Id,
                    Description = pluginDetails.Description,
                    Name = pluginDetails.Name,
                    Categories = pluginDetails.Categories,
                    Versions = pluginDetails.Versions,
                    Inactive = pluginDetails.Inactive
                };
                var iconPath = string.Empty;
                if (string.IsNullOrEmpty(pluginDetails.Icon.MediaUrl))
                {
                    var defaultIconResult = _pluginsController.GetDefaultIcon();

                    if (defaultIconResult is OkObjectResult resultObject && resultObject.StatusCode == 200)
                    {
                        iconPath = resultObject.Value as string;
                    }
                }
                else
                {
                    iconPath = pluginDetails.Icon.MediaUrl;
                }

                privatePlugin.SetIcon(iconPath);
                PrivatePlugins.Add(privatePlugin);
            }
        }

        private PluginFilter ApplyFilters()
        {
            const string statusFilter = "status";
            const string searchFilter = "search";
            IQueryCollection query = Request.Query;
            PluginFilter filters = new PluginFilter
            {
                SortOrder = "asc",
                Status = PluginFilter.StatusValue.All
            };

            if (query.ContainsKey(statusFilter))
            {
                bool isValidType = int.TryParse(query[statusFilter], out int statusValueIndex);
                if (isValidType && Enum.IsDefined(typeof(PluginFilter.StatusValue), statusValueIndex))
                {
                    filters.Status = (PluginFilter.StatusValue)statusValueIndex;
                }
            }

            if (query.ContainsKey(searchFilter))
            {
                filters.Query = query[searchFilter];
            }

            return filters;
        }
    }
}