using AppStoreIntegrationServiceManagement.Model.Notifications;

namespace AppStoreIntegrationServiceManagement.Repository.Interface
{
    public interface INotificationsManager
    {
        Task<IDictionary<string, IEnumerable<PushNotification>>> GetNotifications();
        Task SaveNotifications(IDictionary<string, IEnumerable<PushNotification>> notifications);
    }
}
