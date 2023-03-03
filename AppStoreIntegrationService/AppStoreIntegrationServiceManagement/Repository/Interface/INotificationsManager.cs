using AppStoreIntegrationServiceManagement.Model.Notifications;

namespace AppStoreIntegrationServiceManagement.Repository.Interface
{
    public interface INotificationsManager
    {
        Task<IDictionary<string, IEnumerable<Notification>>> GetNotifications();
        Task SaveNotifications(IDictionary<string, IEnumerable<Notification>> notifications);
    }
}
