using System.ComponentModel.DataAnnotations;
using AppStoreIntegrationServiceCore.DataBase;

namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class ProfileModel : IEquatable<ProfileModel>
    {
        public ProfileModel() { }

        public ProfileModel(IdentityUserExtended user)
        {
            Email = user.Email;
            UserName = user.UserName;
        }

        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address!")]
        public string Email { get; set; }
        [Display(Name = "User role")]
        public string Id { get; set; }
        public bool IsUsernameEditable { get; set; }

        public bool Equals(ProfileModel other)
        {
            return UserName == other?.UserName && Email == other?.Email;
        }
    }
}
