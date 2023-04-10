using AppStoreIntegrationServiceManagement.Model.Plugins;

namespace AppStoreIntegrationServiceManagement.Model.Notifications
{
    public class NotificationModel
    {
        public IEnumerable<PushNotification> Notifications { get; set; }
        public IEnumerable<FilterItem> Filters { get; set; }
    }
}
