using AppStoreIntegrationServiceCore.DataBase.Interface;
using AppStoreIntegrationServiceManagement.DataBase.Interface;
using AppStoreIntegrationServiceManagement.Filters;
using AppStoreIntegrationServiceManagement.Model;
using AppStoreIntegrationServiceManagement.Model.Notifications;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using AppStoreIntegrationServiceManagement.Repository;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Identity
{
    [Area("Identity")]
    [Authorize]
    [DBSynched]
    [AccountSelect]
    [TechPartnerAgreement]
    public class NotificationsController : CustomController
    {
        private readonly IUserProfilesManager _userProfilesManager;
        private readonly INotificationCenter _notificationCenter;
        private readonly IAccountsManager _accountsManager;

        public NotificationsController
        (
            INotificationCenter notificationCenter,
            IUserProfilesManager userProfilesManager,
            IUserAccountsManager userAccountsManager,
            IAccountsManager accountsManager

        ) : base(userProfilesManager, userAccountsManager, accountsManager)
        {
            _notificationCenter = notificationCenter;
            _userProfilesManager = userProfilesManager;
            _accountsManager = accountsManager;
        }

        [Route("Identity/Account/Notifications")]
        public IActionResult Index()
        {
            var user = _userProfilesManager.GetUser(User);
            return View("Notifications", new NotificationSettingsModel(user));
        }

        [HttpPost]
        public async Task<IActionResult> Update(NotificationSettingsModel model)
        {
            var user = _userProfilesManager.GetUser(User);
            user.EmailNotificationsEnabled = model.EmailNotificationsEnabled;
            user.PushNotificationsEnabled = model.PushNotificationsEnabled;
            await _userProfilesManager.TryUpdateUserProfile(user);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int? id, NotificationStatus status)
        {
            var user = _userProfilesManager.GetUser(User);
            var account = _accountsManager.GetAccountById(user.SelectedAccountId);
            await _notificationCenter.ChangeStatus(ExtendedUser.IsInRole("SystemAdministrator") ? "SystemAdministrator" : account.Name, id, status);
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

            var notifications = !ExtendedUser.PushNotificationsEnabled ? Array.Empty<PushNotification>() : ExtendedUser.IsInRole("SystemAdministrator") switch
            {
                true => await _notificationCenter.GetNotificationsForUser("SystemAdministrator"),
                false => await _notificationCenter.GetNotificationsForUser(ExtendedUser.AccountName)
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
