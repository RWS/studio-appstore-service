using AppStoreIntegrationServiceManagement.Filters;
using AppStoreIntegrationServiceManagement.Model;
using AppStoreIntegrationServiceManagement.Model.DataBase;
using AppStoreIntegrationServiceManagement.Model.Identity;
using AppStoreIntegrationServiceManagement.Model.Notifications;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using AppStoreIntegrationServiceManagement.Repository;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Identity
{
    [Area("Identity")]
    [Authorize]
    [AccountSelected]
    [RoleAuthorize("Administrator", "Developer")]
    public class NotificationsController : CustomController
    {
        private readonly UserManager<IdentityUserExtended> _userManager;
        private readonly AccountsManager _accountsManager;
        private readonly INotificationCenter _notificationCenter;

        public NotificationsController
        (
            UserManager<IdentityUserExtended> userManager, 
            INotificationCenter notificationCenter,
            AccountsManager accountsManager)
        {
            _userManager = userManager;
            _notificationCenter = notificationCenter;
            _accountsManager = accountsManager;
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
        public async Task<IActionResult> ChangeStatus(int? id, NotificationStatus status)
        {
            var user = await _userManager.GetUserAsync(User);
            var account = _accountsManager.GetAccountById(user.SelectedAccountId);
            await _notificationCenter.ChangeStatus(ExtendedUser.IsInRole("Administrator") ? "Administrator" : account.AccountName, id, status);
            return new EmptyResult();
        }

        [HttpPost]
        public async Task<IActionResult> LoadNotifications(NotificationStatus notificationStatus, string notificationQuery)
        {
            var statusItems = new List<string> { "Active", "Complete", "Acknowledged", "Inactive", "All" }.Select(x => new FilterItem
            {
                Id = "Status",
                Label = x,
                Value = $"{(int)Enum.Parse(typeof(NotificationStatus), x)}",
                IsSelected = notificationStatus == (NotificationStatus)Enum.Parse(typeof(NotificationStatus), x)
            });

            var notifications = !ExtendedUser.PushNotificationsEnabled ? Array.Empty<Notification>() : ExtendedUser.HasFullOwnership() switch
            {
                true => await _notificationCenter.GetNotificationsForUser(ExtendedUser.AccountName),
                false => await _notificationCenter.GetNotificationsForUser(User.Identity.Name)
            };

            return PartialView("_NotificationsPartial", new NotificationModel
            {
                Notifications = _notificationCenter.FilterNotifications(notifications, notificationStatus == NotificationStatus.None ? NotificationStatus.Active : notificationStatus, notificationQuery),
                Filters = statusItems.Append(new FilterItem
                {
                    Id = "Notification",
                    Label = notificationQuery,
                    Value = notificationQuery,
                    IsSelected = !string.IsNullOrEmpty(notificationQuery)
                })
            });
        }
    }
}
