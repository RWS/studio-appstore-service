using AppStoreIntegrationServiceManagement.Model.Notifications;
using AppStoreIntegrationServiceManagement.Model.Plugins;

namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class NotificationModel
    {
        public IEnumerable<Notification> Notifications { get; set; }
        public IEnumerable<FilterItem> Filters { get; set; }
    }
}
