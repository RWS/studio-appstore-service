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
            return View("Notifications", new NotificationsModel
            {
                EmailNotificationsEnabled = user.NotificationsEnabled
            });
        }

        [HttpPost]
        public async Task<IActionResult> Update(NotificationsModel notifications)
        {
            var user = await _userManager.GetUserAsync(User);
            user.NotificationsEnabled = notifications.EmailNotificationsEnabled;
            await _userManager.UpdateAsync(user);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _notificationCenter.DeleteNotification(User.Identity.Name, id);
            return new EmptyResult();
        }
    }
}
