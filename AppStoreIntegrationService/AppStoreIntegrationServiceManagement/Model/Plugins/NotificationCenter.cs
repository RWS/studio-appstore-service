using NuGet.Protocol.Plugins;
using System;
using System.Net.Mail;

namespace AppStoreIntegrationServiceManagement.Model.Plugins
{
    public class NotificationCenter
    {
        private readonly string _host;
        private readonly string _scheme;

        public NotificationCenter(IHttpContextAccessor context)
        {
            _host = context.HttpContext.Request.Host.Value;
            _scheme = context.HttpContext.Request.Scheme;
        }

        private const string Notification = @"<body style=""border: 1px solid lightgray; padding:20px; width: 500px"">
                                                <div style=""text-align: center;""><b>{0}</b></div>
                                                <div style=""text-align: center;""><b><br></b></div>
                                                <div style=""text-align: center;""><img src=""{1}"" width=""250""></div>
                                                <div style=""text-align: center;""><br></div>
                                                <div style=""text-align: center;""><b>{2}</b></div>
                                                <div style=""text-align: center;""><b><br></b></div>
                                                <div style=""text-align: center;""><a href=""{3}"">View</a><b><br></b></div>
                                              </body>";

        public void SendEmail(string message, string subject)
        {
            try
            {
                var mail = new MailMessage
                {
                    From = new MailAddress("noreply@sdl.com"),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };

                mail.To.Add("catot@sdl.com");
                mail.To.Add("tot.catalin98@gmail.com");
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
        }

        public string GetDeletionRequestNotification(string icon, string pluginName, int? id = null)
        {
            if (id == null)
            {
                return string.Format(Notification, "There is a new deletion request for a plugin!", icon, pluginName, $"{_scheme}://{_host}/Plugins?Query={pluginName}");
            }

            return string.Format(Notification, "There is a new deletion request for a plugin version!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Versions");
        }

        public string GetDeletionApprovedNotification(string icon, string pluginName, int? id = null)
        {
            if (id == null)
            {
                return string.Format(Notification, "Plugin deletion request was accepted!", icon, pluginName, $"{_scheme}://{_host}/Plugins?Query={pluginName}");
            }

            return string.Format(Notification, "Plugin version deletion request was accepted!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Versions");
        }

        public string GetDeletionRejectedNotification(string icon, string pluginName, int? id = null)
        {
            if (id == null)
            {
                return string.Format(Notification, "Plugin deletion request was rejected!", icon, pluginName, $"{_scheme}://{_host}/Plugins?Query={pluginName}");
            }

            return string.Format(Notification, "Plugin deletion request was rejected!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Versions");
        }

        public string GetReviewRequestNotification(string icon, string pluginName, int id, string versionId = null)
        {
            if (versionId == null)
            {
                return string.Format(Notification, "There is a new plugin awaiting approval!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Pending/{id}");
            }

            return string.Format(Notification, "There is a new plugin version awaiting approval!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Versions/Pending/{versionId}");
        }

        public string GetApprovedNotification(string icon, string pluginName, int id, string versionId = null)
        {
            if (versionId == null)
            {
                return string.Format(Notification, "Your submitted plugin was approved!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}");
            }

            return string.Format(Notification, "Your submitted plugin version was approved!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Versions/Details/{versionId}");
        }

        public string GetRejectedNotification(string icon, string pluginName, int id, string versionId = null)
        {
            if (versionId == null)
            {
                return string.Format(Notification, "Your submitted plugin was rejected!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Draft/{id}");
            }

            return string.Format(Notification, "Your submitted plugin version was rejected!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Versions/Draft/{versionId}");
        }

        public string GetNewCommentNotification(string icon, string pluginName, int id, string versionId = null)
        {
            if (versionId == null)
            {
                return string.Format(Notification, "There are new comments for plugin!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Comments");
            }

            return string.Format(Notification, "There are new comments for plugin version!", icon, pluginName, $"{_scheme}://{_host}/Plugins/Edit/{id}/Versions/Edit/{versionId}/Comments");
        }
    }
}
