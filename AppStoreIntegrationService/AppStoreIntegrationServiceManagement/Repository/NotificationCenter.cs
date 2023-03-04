using AppStoreIntegrationServiceCore.Repository.Interface;
using Microsoft.AspNetCore.Identity;
using System.Resources;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Data;
using System.Text.RegularExpressions;
using AppStoreIntegrationServiceManagement.Model.DataBase;
using AppStoreIntegrationServiceManagement.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model.Notifications;
using System.Security.Claims;

namespace AppStoreIntegrationServiceManagement.Repository
{
    public enum NotificationStatus
    {
        Active = 0,
        Complete,
        Acknowledged,
        Inactive,
        All
    }

    public enum NotificationTemplate
    {
        PluginDeletionRequest = 0,
        VersionDeletionRequest,
        PluginDeletionApproved,
        VersionDeletionApproved,
        PluginDeletionRejected,
        VersionDeletionRejected,
        PluginReviewRequest,
        VersionReviewRequest,
        PluginApprovedRequest,
        VersionApprovedRequest,
        PluginRejectedRequest,
        VersionRejectedRequest,
        NewPluginComment,
        NewVersionComment
    }

    public class NotificationCenter : INotificationCenter
    {
        private readonly UserManager<IdentityUserExtended> _userManager;
        private readonly INotificationsManager _notificationsManager;
        private readonly AccountsManager _accountsManager;
        private readonly UserAccountsManager _userAccountsManager;
        private readonly SendGridClient _sendGridClient;
        private readonly string _host;

        public NotificationCenter
        (
            IHttpContextAccessor context,
            UserManager<IdentityUserExtended> userManager,
            IConfigurationSettings configurationSettings,
            INotificationsManager notificationsManager,
            AccountsManager accountsManager,
            UserAccountsManager userAccountsManager
        )
        {
            _userManager = userManager;
            _notificationsManager = notificationsManager;
            _accountsManager = accountsManager;
            _userAccountsManager = userAccountsManager;
            _host = context.HttpContext.Request.Host.Value;
            var sendGridKey = configurationSettings.SendGridAPIKey;
            if (string.IsNullOrEmpty(sendGridKey))
            {
                return;
            }

            _sendGridClient = new SendGridClient(configurationSettings.SendGridAPIKey);
        }

        private async Task SendEmail(string message, string emailAddress)
        {
            var email = MailHelper.CreateSingleEmail(new EmailAddress("catot@sdl.com"), new EmailAddress(emailAddress), "RWS Plugin update", null, message);
            if (_sendGridClient == null)
            {
                return;
            }

            _ = await _sendGridClient.SendEmailAsync(email);
        }

        public async Task Broadcast(string message, string developerName = null)
        {
            try
            {
                if (developerName == null)
                {
                    foreach (var user in _userManager.Users)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        if (user.EmailNotificationsEnabled && roles[0] == "Administrator")
                        {
                            await SendEmail(message, user.Email);
                        }
                    }

                    return;
                }

                var account = _accountsManager.GetAccountByName(developerName);
                foreach (var user in _userManager.Users)
                {
                    if (user.EmailNotificationsEnabled && _userAccountsManager.BelongsTo(user, account.Id))
                    {
                        await SendEmail(message, user.Email);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task Push(string message, string developerName = null)
        {
            developerName ??= "Administrator";
            var notifications = await GetAllNotifications();
            if (notifications.TryGetValue(developerName, out var userNotifications))
            {
                notifications[developerName] = userNotifications.Append(new Notification
                {
                    Id = userNotifications.Count() + 1,
                    Content = message,
                });

                await _notificationsManager.SaveNotifications(notifications);
                return;
            }

            notifications.TryAdd(developerName, new List<Notification> { new Notification { Content = message } });
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

        public async Task<IEnumerable<Notification>> GetNotificationsForUser(ClaimsPrincipal principal)
        {
            var notifications = await GetAllNotifications();
            var user = await _userManager.GetUserAsync(principal);
            var roles = await _userManager.GetRolesAsync(user);
            var account = _accountsManager.GetAccountById(user.SelectedAccountId);
            var username = roles.FirstOrDefault() == "Administrator" ? roles.FirstOrDefault() : account.AccountName;

            if (notifications.TryGetValue(username, out var userNotifications))
            {
                if (user.PushNotificationsEnabled)
                {
                    return userNotifications;
                }

                return Enumerable.Empty<Notification>();
            }

            return Enumerable.Empty<Notification>();
        }

        public async Task<bool> HasNewNotifications(ClaimsPrincipal principal)
        {
            var notifications = await GetAllNotifications();
            var user = await _userManager.GetUserAsync(principal);
            var roles = await _userManager.GetRolesAsync(user);
            var account = _accountsManager.GetAccountById(user.SelectedAccountId);
            var username = roles.FirstOrDefault() == "Administrator" ? roles.FirstOrDefault() : account.AccountName;

            if (notifications.TryGetValue(username, out var userNotifications))
            {
                if (user.PushNotificationsEnabled)
                {
                    return userNotifications.Any(x => x.Status == NotificationStatus.Active);
                }
            }

            return false;
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

        public IEnumerable<Notification> FilterNotifications(IEnumerable<Notification> notifications, NotificationStatus status = NotificationStatus.Active, string query = null)
        {
            if (status != NotificationStatus.All)
            {
                notifications = notifications.Where(x => x.Status == status);
            }

            if (string.IsNullOrEmpty(query))
            {
                return notifications;
            }

            return notifications.Where(x => Regex.IsMatch(x.Content, query, RegexOptions.IgnoreCase));
        }

        public string GetNotification(NotificationTemplate notificationTemplate, bool isEmailNotification, string icon, string pluginName, int pluginId, string versionId = null)
        {
            var resourceManager = new ResourceManager("AppStoreIntegrationServiceManagement.TemplateResource", typeof(TemplateResource).Assembly);
            var notificationType = isEmailNotification ? "Email" : "Push";
            var notification = resourceManager.GetString($"{notificationTemplate}{notificationType}Notification");

            return notificationTemplate switch
            {
                NotificationTemplate.PluginDeletionRequest => string.Format(notification, icon, pluginName, _host, pluginName, DateTime.Now),
                NotificationTemplate.PluginDeletionApproved => string.Format(notification, icon, pluginName, _host, pluginName, DateTime.Now),
                NotificationTemplate.PluginDeletionRejected => string.Format(notification, icon, pluginName, _host, pluginName, DateTime.Now),
                NotificationTemplate.VersionReviewRequest => string.Format(notification, icon, pluginName, _host, pluginId, versionId, DateTime.Now),
                NotificationTemplate.VersionApprovedRequest => string.Format(notification, icon, pluginName, _host, pluginId, versionId, DateTime.Now),
                NotificationTemplate.VersionRejectedRequest => string.Format(notification, icon, pluginName, _host, pluginId, versionId, DateTime.Now),
                NotificationTemplate.NewVersionComment => string.Format(notification, icon, pluginName, _host, pluginId, versionId, DateTime.Now),
                NotificationTemplate.VersionDeletionRequest => string.Format(notification, icon, pluginName, _host, pluginId, DateTime.Now),
                NotificationTemplate.VersionDeletionApproved => string.Format(notification, icon, pluginName, _host, pluginId, DateTime.Now),
                NotificationTemplate.VersionDeletionRejected => string.Format(notification, icon, pluginName, _host, pluginId, DateTime.Now),
                NotificationTemplate.PluginReviewRequest => string.Format(notification, icon, pluginName, _host, pluginId, DateTime.Now),
                NotificationTemplate.PluginApprovedRequest => string.Format(notification, icon, pluginName, _host, pluginId, DateTime.Now),
                NotificationTemplate.PluginRejectedRequest => string.Format(notification, icon, pluginName, _host, pluginId, DateTime.Now),
                NotificationTemplate.NewPluginComment => string.Format(notification, icon, pluginName, _host, pluginId, DateTime.Now),
                _ => null
            };
        }

        private async Task<IDictionary<string, IEnumerable<Notification>>> GetAllNotifications()
        {
            return await _notificationsManager.GetNotifications();
        }
    }
}
