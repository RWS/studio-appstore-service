using AppStoreIntegrationServiceCore.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Settings
{
    [Authorize(Policy = "IsAdmin")]
    [Area("Settings")]
    public class GeneralSettingsController : Controller
    {
        private readonly IWritableOptions<SiteSettings> _options;

        public GeneralSettingsController(IWritableOptions<SiteSettings> options)
        {
            _options = options;
        }

        [Route("Settings")]
        public IActionResult Index()
        {
            return View(new SiteSettings { Name = _options.Value.Name });
        }

        [HttpPost]
        public IActionResult Update(SiteSettings settings)
        {
            _options.SaveOption(new SiteSettings { Name = settings.Name });
            _options.Value.Name = settings.Name;
            return RedirectToAction("Index");
        }
    }
}
