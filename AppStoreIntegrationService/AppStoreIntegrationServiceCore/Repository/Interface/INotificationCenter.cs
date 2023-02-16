using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface INotificationCenter
    {
        Task SendEmail(string message, string emailAddress);
        Task Broadcast(string message);
        Task Push(string message, string username = null);
        Task ChangeStatus(string username, int? id, NotificationStatus status);
        Task<IEnumerable<Notification>> GetNotificationsForUser(string username, string role = null);
        Task<bool> HasNewNotifications(string username, string role = null);
        Task DeleteNotification(string username, int id);
        IEnumerable<Notification> FilterNotifications(IEnumerable<Notification> notifications, NotificationStatus status = NotificationStatus.All, string query = null);
        string GetNotification(NotificationTemplate notificationTemplate, bool isEmailNotification, string icon, string pluginName, int pluginId, string versionId = null);
    }
}
