using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface INotificationsManager
    {
        Task<IDictionary<string, IEnumerable<Notification>>> GetNotifications();
        Task SaveNotifications(IDictionary<string, IEnumerable<Notification>> notifications);
    }
}
