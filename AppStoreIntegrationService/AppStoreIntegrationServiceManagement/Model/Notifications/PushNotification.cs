using AppStoreIntegrationServiceManagement.Repository;

namespace AppStoreIntegrationServiceManagement.Model.Notifications
{
    public class PushNotification : EmailNotification
    {
        public PushNotification(EmailNotification notification) 
        {
            ImageSource = notification.ImageSource;
            Title = notification.Title;
            Message = notification.Message;
            CallToActionUrl = notification.CallToActionUrl;
            Author = notification.Author;
        }

        public int Id { get; set; }
        public NotificationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public override string ToHtml()
        {
            return string.Format(TemplateResource.PushNotificationTemplate, ImageSource, Title, Message, CallToActionUrl, CreatedAt);
        }
    }
}
