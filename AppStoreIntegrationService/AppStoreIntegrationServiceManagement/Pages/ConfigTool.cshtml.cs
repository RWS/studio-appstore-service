using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppStoreIntegrationServiceManagement
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ConfigToolModel : PageModel
    {
        private readonly IPluginRepository _pluginRepository;
        private readonly IHttpContextAccessor _context;

        [BindProperty]
        public List<PrivatePlugin> PrivatePlugins { get; set; }

        [BindProperty]
        public PrivatePlugin PrivatePlugin { get; set; }

        [BindProperty]
        public int Id { get; set; }
        [BindProperty]
        public string Name { get; set; }

        public ConfigToolModel(IPluginRepository pluginRepository, IHttpContextAccessor context)
        {
            _pluginRepository = pluginRepository;
            _context = context;
        }

        public async Task OnGetAsync()
        {
            PrivatePlugins = new List<PrivatePlugin>();
            PluginFilter pluginsFilters = ApplyFilters();
            List<PluginDetails> pluginsList = await _pluginRepository.GetAll(pluginsFilters.SortOrder);
            _pluginRepository.SearchPlugins(pluginsList, pluginsFilters);
            InitializePrivatePlugins(_pluginRepository.SearchPlugins(pluginsList, pluginsFilters));
        }

        public async Task<IActionResult> OnGetShowDeleteModal(int id, string name)
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
            await _pluginRepository.RemovePlugin(Id);
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
                string iconPath = string.IsNullOrEmpty(pluginDetails.Icon.MediaUrl) ? GetDefaultIcon() : pluginDetails.Icon.MediaUrl;
                privatePlugin.SetIcon(iconPath);
                PrivatePlugins.Add(privatePlugin);
            }
        }

        private PluginFilter ApplyFilters()
        {
            const string statusFilter = "status";
            const string searchFilter = "search";
            IQueryCollection query = Request.Query;
            PluginFilter filters = new ()
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

        public string GetDefaultIcon()
        {
            var scheme = _context.HttpContext?.Request?.Scheme;
            var host = _context.HttpContext?.Request?.Host.Value;
            return $"{scheme}://{host}/images/plugin.ico";
        }
    }
}