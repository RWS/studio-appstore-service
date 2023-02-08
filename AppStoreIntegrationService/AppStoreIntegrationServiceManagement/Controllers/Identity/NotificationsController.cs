using AppStoreIntegrationServiceManagement.Model.Identity;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Identity
{
    [Area("Identity")]
    [Authorize(Roles = "Administrator,Developer")]
    public class NotificationsController : Controller
    {
        private readonly UserManager<IdentityUserExtended> _userManager;
        private readonly NotificationCenter _notificationCenter;

        public NotificationsController(UserManager<IdentityUserExtended> userManager, NotificationCenter notificationCenter)
        {
            _userManager = userManager;
            _notificationCenter = notificationCenter;
        }

        [Route("Identity/Account/Notifications")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            return View("Notifications", (user.EmailNotificationsEnabled, user.PushNotificationsEnabled));
        }

        [HttpPost]
        public async Task<IActionResult> Update(bool emailNotificationsEnabled, bool pushNotificationsEnabled)
        {
            var user = await _userManager.GetUserAsync(User);
            user.EmailNotificationsEnabled = emailNotificationsEnabled;
            user.PushNotificationsEnabled = pushNotificationsEnabled;
            await _userManager.UpdateAsync(user);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, bool removeAll = false)
        {
            await _notificationCenter.DeleteNotification(User.Identity.Name, id, removeAll);
            return new EmptyResult();
        }
    }
}
