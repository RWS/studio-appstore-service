using AppStoreIntegrationServiceManagement.Repository;

namespace AppStoreIntegrationServiceManagement.Model.Notifications
{
    public class Notification
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public NotificationStatus Status { get; set; }
    }
}
