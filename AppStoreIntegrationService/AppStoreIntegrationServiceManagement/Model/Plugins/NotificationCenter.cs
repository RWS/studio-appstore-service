using AppStoreIntegrationServiceCore;
using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model.Identity;
using Microsoft.AspNetCore.Identity;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AppStoreIntegrationServiceManagement.Model.Plugins
{
    public class NotificationCenter
    {
        private readonly UserManager<IdentityUserExtended> _userManager;
        private readonly IConfiguration _configuration;
        private readonly INotificationsManager _notificationsManager;
        private readonly string _host;
        private readonly string _scheme;

        public NotificationCenter
        (
            IHttpContextAccessor context,
            UserManager<IdentityUserExtended> userManager,
            IConfiguration configuration,
            INotificationsManager notificationsManager
        )
        {
            _userManager = userManager;
            _configuration = configuration;
            _notificationsManager = notificationsManager;
            _host = context.HttpContext.Request.Host.Value;
            _scheme = context.HttpContext.Request.Scheme;
        }

        public async Task SendEmail(string message, string subject, string emailAddress)
        {
            //try
            //{
            //    var client = new SendGridClient(_configuration.GetSection("SendGridApiKey").Value);
            //    var email = MailHelper.CreateSingleEmail(new EmailAddress("catot@sdl.com"), new EmailAddress(emailAddress), subject, null, message);
            //    var response = await client.SendEmailAsync(email);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //}
        }

        public async Task Broadcast(string message, string subject)
        {
            //try
            //{
            //    var client = new SendGridClient(_configuration.GetSection("SendGridApiKey").Value);
            //    foreach (var user in _userManager.Users)
            //    {
            //        var roles = await _userManager.GetRolesAsync(user);
            //        if (user.NotificationsEnabled && roles[0] == "Administrator")
            //        {
            //            var email = MailHelper.CreateSingleEmail(new EmailAddress("catot@sdl.com"), new EmailAddress(user.Email), subject, null, message);
            //            var response = await client.SendEmailAsync(email);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //}
        }

        public async Task Push(string message, string username = null)
        {
            var notifications = await GetAllNotifications();

            if (username == null)
            {
                foreach (var user in _userManager.Users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (user.PushNotificationsEnabled && roles[0] == "Administrator")
                    {
                        Push(notifications, message, user.UserName);
                    }
                }

                return;
            }

            if (!(await _userManager.FindByNameAsync(username)).PushNotificationsEnabled)
            {
                return;
            }

            Push(notifications, message, username);
        }

        public async Task<IEnumerable<Notification>> GetNotificationsForUser(string username)
        {
            var notifications = await GetAllNotifications();

            if (notifications.TryGetValue(username, out var userNotifications))
            {
                return userNotifications;
            }

            return Enumerable.Empty<Notification>();
        }

        public async Task DeleteNotification(string username, int id, bool removeAll = false)
        {
            var notifications = await GetAllNotifications();

            if (!notifications.TryGetValue(username, out var userNotifications))
            {
                return;
            }

            notifications[username] = removeAll ? new List<Notification>() : userNotifications.Where(x => x.Id != id);
            await _notificationsManager.SaveNotifications(notifications);
        }

        public (string, string) GetDeletionRequestNotification(string icon, string pluginName, int? id = null)
        {
            if (id == null)
            {
                return (string.Format(TemplateResource.EmailNotificationTemplate, "There is a new deletion request for a plugin!", icon, pluginName, $"{_scheme}://{_host}/Plugins?Query={pluginName}"),
                        string.Format(TemplateResource.PushNotificationTemplate, "There is a new deletion request for a plugin!", icon, pluginName, $"{_scheme}://{_host}/Plugins?Query={pluginName}", DateTime.Now));
            }

            return (string.Format(TemplateResource.EmailNotificationTemplate, "There is a new deletion request for a plugin version!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Versions"),
                    string.Format(TemplateResource.PushNotificationTemplate, "There is a new deletion request for a plugin!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Versions", DateTime.Now));
        }

        public (string, string) GetDeletionApprovedNotification(string icon, string pluginName, int? id = null)
        {
            if (id == null)
            {
                return (string.Format(TemplateResource.EmailNotificationTemplate, "Plugin deletion request was accepted!", icon, pluginName, $"{_scheme}://{_host}/Plugins?Query={pluginName}"),
                        string.Format(TemplateResource.PushNotificationTemplate, "Plugin deletion request was accepted!", icon, pluginName, $"{_scheme}://{_host}/Plugins?Query={pluginName}", DateTime.Now));
            }

            return (string.Format(TemplateResource.EmailNotificationTemplate, "Plugin version deletion request was accepted!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Versions"),
                    string.Format(TemplateResource.PushNotificationTemplate, "Plugin version deletion request was accepted!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Versions"));
        }

        public (string, string) GetDeletionRejectedNotification(string icon, string pluginName, int? id = null)
        {
            if (id == null)
            {
                return (string.Format(TemplateResource.EmailNotificationTemplate, "Plugin deletion request was rejected!", icon, pluginName, $"{_scheme}://{_host}/Plugins?Query={pluginName}"),
                        string.Format(TemplateResource.PushNotificationTemplate, "Plugin deletion request was rejected!", icon, pluginName, $"{_scheme}://{_host}/Plugins?Query={pluginName}", DateTime.Now));
            }

            return (string.Format(TemplateResource.EmailNotificationTemplate, "Plugin version deletion request was rejected!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Versions"),
                    string.Format(TemplateResource.PushNotificationTemplate, "Plugin version deletion request was rejected!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Versions", DateTime.Now));
        }

        public (string, string) GetReviewRequestNotification(string icon, string pluginName, int id, string versionId = null)
        {
            if (versionId == null)
            {
                return (string.Format(TemplateResource.EmailNotificationTemplate, "There is a new plugin awaiting approval!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Pending/{id}"),
                        string.Format(TemplateResource.PushNotificationTemplate, "There is a new plugin awaiting approval!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Pending/{id}", DateTime.Now));
            }

            return (string.Format(TemplateResource.EmailNotificationTemplate, "There is a new plugin version awaiting approval!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Versions/Pending/{versionId}"),
                    string.Format(TemplateResource.PushNotificationTemplate, "There is a new plugin version awaiting approval!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Versions/Pending/{versionId}", DateTime.Now));
        }

        public (string, string) GetApprovedNotification(string icon, string pluginName, int id, string versionId = null)
        {
            if (versionId == null)
            {
                return (string.Format(TemplateResource.EmailNotificationTemplate, "Your submitted plugin was approved!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}"),
                        string.Format(TemplateResource.PushNotificationTemplate, "Your submitted plugin was approved!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}", DateTime.Now));
            }

            return (string.Format(TemplateResource.EmailNotificationTemplate, "Your submitted plugin version was approved!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Versions/Details/{versionId}"),
                    string.Format(TemplateResource.PushNotificationTemplate, "Your submitted plugin version was approved!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Versions/Details/{versionId}", DateTime.Now));
        }

        public (string, string) GetRejectedNotification(string icon, string pluginName, int id, string versionId = null)
        {
            if (versionId == null)
            {
                return (string.Format(TemplateResource.EmailNotificationTemplate, "Your submitted plugin was rejected!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Draft/{id}"),
                        string.Format(TemplateResource.PushNotificationTemplate, "Your submitted plugin was rejected!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Draft/{id}", DateTime.Now));
            }

            return (string.Format(TemplateResource.EmailNotificationTemplate, "Your submitted plugin version was rejected!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Versions/Draft/{versionId}"),
                    string.Format(TemplateResource.PushNotificationTemplate, "Your submitted plugin version was rejected!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Versions/Draft/{versionId}", DateTime.Now));
        }

        public (string, string) GetNewCommentNotification(string icon, string pluginName, int id, string versionId = null)
        {
            if (versionId == null)
            {
                return (string.Format(TemplateResource.EmailNotificationTemplate, "There are new comments for plugin!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Comments"),
                        string.Format(TemplateResource.PushNotificationTemplate, "There are new comments for plugin!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Comments", DateTime.Now));
            }

            return (string.Format(TemplateResource.EmailNotificationTemplate, "There are new comments for plugin version!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Versions/Edit/{versionId}/Comments"),
                    string.Format(TemplateResource.PushNotificationTemplate, "There are new comments for plugin version!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Versions/Edit/{versionId}/Comments", DateTime.Now));
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
