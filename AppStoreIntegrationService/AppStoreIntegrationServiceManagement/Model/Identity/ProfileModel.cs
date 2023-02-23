using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class ProfileModel : IEquatable<ProfileModel>
    {
        public ProfileModel() { }

        public ProfileModel(IdentityUserExtended user, string role)
        {
            UserName = user.UserName;
            UserRole = role;
        }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address!")]
        public string UserName { get; set; }
        [Display(Name = "User role")]
        public string UserRole { get; set; }
        public string Id { get; set; }
        public bool IsUsernameEditable { get; set; }
        public bool IsUserRoleEditable { get; set; }

        public bool Equals(ProfileModel other)
        {
            return UserName == other?.UserName && UserRole == other?.UserRole;
        }
    }
}
