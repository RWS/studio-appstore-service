using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class ProfileModel : IEquatable<ProfileModel>
    {
        public ProfileModel() { }

        public ProfileModel(IdentityUserExtended user, string role)
        {
            Username = user.UserName;
            Email = user.Email;
            UserRole = role;
        }

        [Required]
        public string Username { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address!")]
        public string Email { get; set; }
        [Display(Name = "User role")]
        public string UserRole { get; set; }
        public string Id { get; set; }
        public bool IsUsernameEditable { get; set; }
        public bool IsUserRoleEditable { get; set; }

        public bool Equals(ProfileModel other)
        {
            return Username == other?.Username &&
                   Email == other?.Email &&
                   UserRole == other?.UserRole;
        }
    }
}
