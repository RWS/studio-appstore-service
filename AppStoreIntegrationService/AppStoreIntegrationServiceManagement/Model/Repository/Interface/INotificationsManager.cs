using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceManagement.Model.Repository.Interface
{
    public interface INotificationsManager
    {
        Task<IDictionary<string, IEnumerable<Notification>>> GetNotifications();
        Task SaveNotifications(IDictionary<string, IEnumerable<Notification>> notifications);
    }
}
