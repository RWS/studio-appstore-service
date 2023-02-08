using Microsoft.AspNetCore.Identity;

namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class IdentityUserExtended : IdentityUser
    {
        public bool EmailNotificationsEnabled { get; set; }
        public bool PushNotificationsEnabled { get; set; }
    }
}
