using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model.Identity;
using Microsoft.AspNetCore.Identity;
using System.Resources;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Reflection;
using System.Net.Mail;
using System;
using AppStoreIntegrationServiceCore;

namespace AppStoreIntegrationServiceManagement.Model.Plugins
{
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

    public class NotificationCenter
    {
        private readonly UserManager<IdentityUserExtended> _userManager;
        private readonly IConfiguration _configuration;
        private readonly INotificationsManager _notificationsManager;
        private readonly IWebHostEnvironment _environment;
        private readonly string _host;

        public NotificationCenter
        (
            IHttpContextAccessor context,
            UserManager<IdentityUserExtended> userManager,
            IConfiguration configuration,
            INotificationsManager notificationsManager,
            IWebHostEnvironment environment
        )
        {
            _userManager = userManager;
            _configuration = configuration;
            _notificationsManager = notificationsManager;
            _environment = environment;
            _host = context.HttpContext.Request.Host.Value;
        }

        public async Task SendEmail(string message, string emailAddress)
        {
            try
            {
                var mail = new MailMessage
                {
                    From = new MailAddress("noreply@sdl.com"),
                    Subject = "RWS Plugin update",
                    Body = message,
                    IsBodyHtml = true
                };

                mail.To.Add(emailAddress);
                var smtp = new SmtpClient
                {
                    Host = "mailuk.sdl.corp"
                };

                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
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

        public async Task Broadcast(string message)
        {
            try
            {
                var mail = new MailMessage
                {
                    From = new MailAddress("noreply@sdl.com"),
                    Subject = "RWS Plugin update",
                    Body = message,
                    IsBodyHtml = true
                };

                foreach (var user in _userManager.Users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (user.EmailNotificationsEnabled && roles[0] == "Administrator")
                    {
                        mail.To.Add(user.Email);
                    }
                }

                var smtp = new SmtpClient
                {
                    Host = "mailuk.sdl.corp"
                };

                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
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
                foreach (var identityUser in _userManager.Users)
                {
                    var roles = await _userManager.GetRolesAsync(identityUser);
                    if (identityUser.PushNotificationsEnabled && roles[0] == "Administrator")
                    {
                        Push(notifications, message, identityUser.UserName);
                    }
                }

                return;
            }

            var user = await _userManager.FindByNameAsync(username);
            if (!user.PushNotificationsEnabled)
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
