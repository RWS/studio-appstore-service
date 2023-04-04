using AppStoreIntegrationServiceManagement.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Settings
{
    [Area("Settings")]
    [Authorize]
    [DBSynched]
    [AccountSelect]
    [TechPartnerAgreement]
    public class PreferencesController : Controller
    {
        public IActionResult Index()
        {
            return View("BasicCustomization");
        }

        public IActionResult Advanced()
        {
            return View("AdvancedCustomization");
        }
    }
}
