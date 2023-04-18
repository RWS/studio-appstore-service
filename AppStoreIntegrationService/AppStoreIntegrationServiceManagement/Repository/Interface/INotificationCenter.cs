using AppStoreIntegrationServiceManagement.Model.Notifications;
using Microsoft.AspNetCore.Identity;

namespace AppStoreIntegrationServiceManagement.Repository.Interface
{
    public interface INotificationCenter
    {
        Task<IdentityResult> Broadcast(EmailNotification notification, params string[] roles);
        Task<IdentityResult> SendEmail(EmailNotification notification);
        Task Push(PushNotification notification);
        Task ChangeStatus(string username, int? id, NotificationStatus status);
        Task<IEnumerable<PushNotification>> GetNotificationsForUser(string username);
        Task<int> GetNotificationsCount(string username);
        Task DeleteNotification(string username, int id);
        IEnumerable<PushNotification> FilterNotifications(IEnumerable<PushNotification> notifications, NotificationStatus status = NotificationStatus.All, string query = null);
    }
}