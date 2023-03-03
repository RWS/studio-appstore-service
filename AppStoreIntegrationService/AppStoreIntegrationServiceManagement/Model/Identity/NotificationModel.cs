using AppStoreIntegrationServiceManagement.Model.Notifications;
using AppStoreIntegrationServiceManagement.Model.Plugins;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class NotificationModel
    {
        public IEnumerable<Notification> Notifications { get; set; }
        public IEnumerable<FilterItem> Filters { get; set; }
        public SelectList StatusListItems { get; set; }
    }
}
