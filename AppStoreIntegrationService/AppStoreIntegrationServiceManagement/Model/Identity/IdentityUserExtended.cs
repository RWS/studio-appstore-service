using Microsoft.AspNetCore.Identity;

namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class IdentityUserExtended : IdentityUser
    {
        public bool NotificationsEnabled { get; set; }
    }
}
