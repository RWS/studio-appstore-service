using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceManagement.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Settings
{
    [Authorize(Policy = "IsAdmin")]
    [Route("Settings/[controller]")]
    public class SiteNameController : Controller
    {
        private readonly IWritableOptions<SiteSettings> _options;

        public SiteNameController(IWritableOptions<SiteSettings> options)
        {
            _options = options;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return View("Views/Settings/SiteName.cshtml", new SiteNameModel { Name = _options.Value.Name });
        }

        [HttpPost]
        public IActionResult Update(SiteNameModel settings)
        {
            _options.SaveOption(new SiteSettings { Name = settings.Name });
            _options.Value.Name = settings.Name;
            return Redirect("SiteName");
        }
    }
}
