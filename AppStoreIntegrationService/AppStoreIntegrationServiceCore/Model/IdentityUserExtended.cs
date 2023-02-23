using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class IdentityUserExtended : IdentityUser
    {
        public bool EmailNotificationsEnabled { get; set; }
        public bool PushNotificationsEnabled { get; set; }
        public bool IsBuiltInAdmin { get; set; }

        public static string GetUserRole(ClaimsIdentity identity)
        {
            return identity.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).FirstOrDefault();
        }
    }
}
