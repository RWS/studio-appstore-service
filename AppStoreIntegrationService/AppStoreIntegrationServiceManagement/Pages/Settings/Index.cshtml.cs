using AppStoreIntegrationServiceCore.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AppStoreIntegrationServiceManagement.Pages.Settings
{
    [Authorize(Policy = "IsAdmin")]
    public class IndexModel : PageModel
    {
        private readonly IWritableOptions<SiteSettings> _options;
        public IndexModel(IWritableOptions<SiteSettings> options)
        {
            _options = options;
        }

        [BindProperty]
        public string SiteName { get; set; }

        public async Task<IActionResult> OnGet()
        {
            SiteName = _options.Value.Name;
            return Page();
        }

        public async Task<IActionResult> OnPostSaveSiteName()
        {
            _options.SaveOption(new SiteSettings { Name = SiteName });
            _options.Value.Name = SiteName;
            return Redirect("Settings");
        }
    }
}
