using AppStoreIntegrationServiceCore.Repository.Interface;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Data;
using System.Text.RegularExpressions;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model.Notifications;
using AppStoreIntegrationServiceManagement.DataBase.Interface;
using AppStoreIntegrationServiceCore.DataBase.Interface;
using Microsoft.AspNetCore.Identity;

namespace AppStoreIntegrationServiceManagement.Repository
{
    public enum NotificationStatus
    {
        None = 0,
        Active,
        Complete,
        Acknowledged,
        Inactive,
        All
    }

    public class NotificationCenter : INotificationCenter
    {
        private readonly IUserProfilesManager _userManager;
        private readonly IUserAccountsManager _userAccountsManager;
        private readonly IAccountsManager _accountsManager;
        private readonly INotificationsManager _notificationsManager;
        private readonly SendGridClient _sendGridClient;

        public NotificationCenter
        (
            IConfigurationSettings configurationSettings,
            INotificationsManager notificationsManager,
            IUserProfilesManager userProfilesManager,
            IUserAccountsManager userAccountsManager,
            IAccountsManager accountsManager
        )
        {
            _notificationsManager = notificationsManager;
            _userManager = userProfilesManager;
            _userAccountsManager = userAccountsManager;
            _accountsManager = accountsManager;
            var sendGridKey = configurationSettings.SendGridAPIKey;
            if (string.IsNullOrEmpty(sendGridKey))
            {
                return;
            }

            _sendGridClient = new SendGridClient(configurationSettings.SendGridAPIKey);
        }

        public async Task<IdentityResult> SendEmail(EmailNotification notification)
        {
            if (_sendGridClient == null)
            {
                return IdentityResult.Success;
            }

            try
            {
                var email = MailHelper.CreateSingleEmail(new EmailAddress("catot@sdl.com"), new EmailAddress(notification.Author), "RWS Account notification", null, notification.ToHtml());
                _ = await _sendGridClient.SendEmailAsync(email);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public async Task<IdentityResult> Broadcast(EmailNotification notification, params string[] roles)
        {
            if (_sendGridClient == null)
            {
                return IdentityResult.Success;
            }

            try
            {
                var account = _accountsManager.GetAccountByName(notification.Author);
                var users = _userAccountsManager.GetUsersFromAccount(account);

                foreach (var user in users)
                {
                    var role = _userAccountsManager.GetUserRoleForAccount(user, account);
                    if (user.EmailNotificationsEnabled && roles.Any(x => x == role.Name))
                    {
                        var email = MailHelper.CreateSingleEmail(new EmailAddress("catot@sdl.com"), new EmailAddress(user.Email), "RWS Plugin update", null, notification.ToHtml());
                        _ = await _sendGridClient.SendEmailAsync(email);
                    }
                }

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public async Task Push(PushNotification notification)
        {
            notification.Status = NotificationStatus.Active;

            var notifications = await GetAllNotifications();
            if (notifications.TryGetValue(notification.Author, out var userNotifications))
            {
                notification.Id = userNotifications.Count() + 1;
                notification.Status = NotificationStatus.Active;
                notifications[notification.Author] = userNotifications.Append(notification);
                await _notificationsManager.SaveNotifications(notifications);
                return;
            }

            notifications.TryAdd(notification.Author, new List<PushNotification> { notification });
            await _notificationsManager.SaveNotifications(notifications);
        }

        public async Task ChangeStatus(string username, int? id, NotificationStatus status)
        {
            var notifications = await GetAllNotifications();

            if (!notifications.TryGetValue(username, out var userNotifications))
            {
                return;
            }

            if (id == null)
            {
                foreach (var notification in userNotifications)
                {
                    notification.Status = status;
                }
            }
            else
            {
                userNotifications.FirstOrDefault(x => x.Id == id).Status = status;
            }

            notifications[username] = userNotifications;
            await _notificationsManager.SaveNotifications(notifications);
        }

        public async Task<IEnumerable<PushNotification>> GetNotificationsForUser(string username)
        {
            var notifications = await GetAllNotifications();

            if (notifications.TryGetValue(username, out var userNotifications))
            {
                return userNotifications;
            }

            return Enumerable.Empty<PushNotification>();
        }

        public async Task<int> GetNotificationsCount(string username)
        {
            var notifications = await GetAllNotifications();

            if (notifications.TryGetValue(username, out var userNotifications))
            {
                return userNotifications.Count(x => x.Status == NotificationStatus.Active);
            }

            return 0;
        }

        public async Task DeleteNotification(string username, int id)
        {
            var notifications = await GetAllNotifications();

            if (!notifications.TryGetValue(username, out var userNotifications))
            {
                return;
            }

            notifications[username] = userNotifications.Where(x => x.Id != id);
            await _notificationsManager.SaveNotifications(notifications);
        }

        public IEnumerable<PushNotification> FilterNotifications(IEnumerable<PushNotification> notifications, NotificationStatus status = NotificationStatus.Active, string query = null)
        {
            if (status != NotificationStatus.All)
            {
                notifications = notifications.Where(x => x.Status == status);
            }

            if (string.IsNullOrEmpty(query))
            {
                return notifications;
            }

            return notifications.Where(x => Regex.IsMatch(x.Message, query, RegexOptions.IgnoreCase));
        }

        private async Task<IDictionary<string, IEnumerable<PushNotification>>> GetAllNotifications()
        {
            return await _notificationsManager.GetNotifications();
        }
    }
}