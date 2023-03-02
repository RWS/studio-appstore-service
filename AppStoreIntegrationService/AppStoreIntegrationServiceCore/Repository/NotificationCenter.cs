using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Microsoft.AspNetCore.Identity;
using System.Resources;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Data;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using AppStoreIntegrationServiceManagement.Model.Identity;

namespace AppStoreIntegrationServiceCore.Repository
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
        private readonly IConfigurationSettings _configurationSettings;
        private readonly INotificationsManager _notificationsManager;
        private readonly string _host;

        public NotificationCenter
        (
            IHttpContextAccessor context,
            UserManager<IdentityUserExtended> userManager,
            IConfigurationSettings configurationSettings,
            INotificationsManager notificationsManager
        )
        {
            _userManager = userManager;
            _configurationSettings = configurationSettings;
            _notificationsManager = notificationsManager;
            _host = context.HttpContext.Request.Host.Value;
        }

        public async Task SendEmail(string message, string emailAddress)
        {
            try
            {
                var sendGridKey = _configurationSettings.SendGridAPIKey;
                if (string.IsNullOrEmpty(sendGridKey))
                {
                    return;
                }

                var client = new SendGridClient(sendGridKey);
                var email = MailHelper.CreateSingleEmail(new EmailAddress("catot@sdl.com"), new EmailAddress(emailAddress), "RWS Plugin update", null, message);
                var response = await client.SendEmailAsync(email);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task Broadcast(string message, string developerName = null)
        {
            try
            {
                var sendGridKey = _configurationSettings.SendGridAPIKey;
                if (string.IsNullOrEmpty(sendGridKey))
                {
                    return;
                }

                var client = new SendGridClient(sendGridKey);
                foreach (var user in _userManager.Users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (user.EmailNotificationsEnabled && roles[0] == "Administrator")
                    {
                        var email = MailHelper.CreateSingleEmail(new EmailAddress("catot@sdl.com"), new EmailAddress(user.Email), "RWS Plugin update", null, message);
                        var response = await client.SendEmailAsync(email);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task Push(string message, string username = null)
        {
            var notifications = await GetAllNotifications();
            if (username == null)
            {
                Push(notifications, message, "Administrator");
                return;
            }

            var user = await _userManager.FindByNameAsync(username);
            if (!user.PushNotificationsEnabled)
            {
                return;
            }

            Push(notifications, message, username);
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

        public async Task<IEnumerable<Notification>> GetNotificationsForUser(string username, string role = null)
        {
            var notifications = await GetAllNotifications();

            if (notifications.TryGetValue(role ?? username, out var userNotifications))
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user.PushNotificationsEnabled)
                {
                    return userNotifications;
                }

                return Enumerable.Empty<Notification>();
            }

            return Enumerable.Empty<Notification>();
        }

        public async Task<bool> HasNewNotifications(string username, string role = null)
        {
            var notifications = await GetAllNotifications();

            if (notifications.TryGetValue(role ?? username, out var userNotifications))
            {
                var user = await _userManager.FindByNameAsync(username);
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
            var resourceManager = new ResourceManager("AppStoreIntegrationServiceCore.TemplateResource", typeof(TemplateResource).Assembly);
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

        private void Push(IDictionary<string, IEnumerable<Notification>> notifications, string message, string username)
        {
            if (notifications.TryGetValue(username, out var userNotifications))
            {
                notifications[username] = userNotifications.Append(new Notification
                {
                    Id = userNotifications.Count() + 1,
                    Content = message,
                });

                _notificationsManager.SaveNotifications(notifications);
                return;
            }

            notifications.TryAdd(username, new List<Notification> { new Notification { Content = message } });
            _notificationsManager.SaveNotifications(notifications);
        }
    }
}
