using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.V2.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Settings
{
    [Authorize(Policy = "IsAdmin")]
    [Area("Settings")]
    public class SettingsController : Controller
    {
        private readonly ISettingsRepository _settingsRepository;

        public SettingsController(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        [Route("Settings")]
        public async Task<IActionResult> Index()
        {
            return View(await _settingsRepository.GetSettings());
        }

        [HttpPost]
        public async Task<IActionResult> Update(SiteSettings settings)
        {
            await _settingsRepository.SaveSettings(settings);
            return RedirectToAction("Index");
        }
    }
}
