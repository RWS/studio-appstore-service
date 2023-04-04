using AppStoreIntegrationServiceCore.DataBase.Models;
using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceManagement.Model.Identity
{
    public class ProfileModel : IEquatable<ProfileModel>
    {
        public ProfileModel() { }

        public ProfileModel(UserProfile user)
        {
            Email = user.Email;
            UserName = user.Name;
        }

        [Required]
        public string UserName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address!")]
        public string Email { get; set; }
        [Display(Name = "User role")]
        public string Id { get; set; }
        public bool IsBuiltInAdmin { get; set; }

        public bool Equals(ProfileModel other)
        {
            return UserName == other?.UserName && Email == other?.Email;
        }
    }
}
