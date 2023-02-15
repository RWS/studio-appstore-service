using AppStoreIntegrationServiceCore.Repository;

namespace AppStoreIntegrationServiceCore.Model
{
    public class Notification
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public NotificationStatus Status { get; set; }
    }
}
