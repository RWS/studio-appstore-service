using AppStoreIntegrationServiceManagement.Model.Notifications;

namespace AppStoreIntegrationServiceManagement.Repository.Interface
{
    public interface INotificationCenter
    {
        Task Broadcast(string message);
        Task SendEmail(string message, string username = null);
        Task Push(string message, string username = null);
        Task ChangeStatus(string username, int? id, NotificationStatus status);
        Task<IEnumerable<Notification>> GetNotificationsForUser(string username);
        Task<int> GetNotificationsCount(string username);
        Task DeleteNotification(string username, int id);
        IEnumerable<Notification> FilterNotifications(IEnumerable<Notification> notifications, NotificationStatus status = NotificationStatus.All, string query = null);
        string GetNotification(NotificationTemplate notificationTemplate, bool isEmailNotification, string icon, string pluginName, int pluginId, string versionId = null);
    }
}
