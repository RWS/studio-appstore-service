using AppStoreIntegrationServiceManagement.Filters;
using AppStoreIntegrationServiceManagement.Model.Settings;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Settings
{
    [Area("Settings")]
    [Authorize]
    [AccountSelect]
    [RoleAuthorize("System Administrator")]
    public class SettingsController : Controller
    {
        private readonly ISettingsManager _settingsManager;

        public SettingsController(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        [Route("Settings")]
        public async Task<IActionResult> Index()
        {
            return View(await _settingsManager.ReadSettings());
        }

        [HttpPost]
        public async Task<IActionResult> Update(SiteSettings settings)
        {
            await _settingsManager.SaveSettings(settings);
            return RedirectToAction("Index");
        }
    }
}
