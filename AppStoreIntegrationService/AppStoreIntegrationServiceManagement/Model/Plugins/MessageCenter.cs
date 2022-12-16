using AppStoreIntegrationServiceCore.Model;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;

namespace AppStoreIntegrationServiceManagement.Model.Plugins
{
    public class MessageCenter
    {
        private readonly MailMessage _mail;
        private readonly SmtpClient _smptClient;

        public MessageCenter(string sender, IEnumerable<string> receivers)
        {
            _mail = new MailMessage
            {
                From = new MailAddress(sender),
                IsBodyHtml = true
            };

            foreach (var receiver in receivers)
            {
                _mail.To.Add(receiver);
            }

            _smptClient = new SmtpClient
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true
            };

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    _smptClient.Host = ip.ToString();
                }
            }
        }

        public bool TryBroadcast(ExtendedPluginDetails<PluginVersion<string>> plugin)
        {
            _mail.Subject = "New plugin is waiting to be reviewed";
            _mail.Body = $"{plugin.Name}";

            try
            {
                _smptClient.Send(_mail);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
