using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppStoreIntegrationServiceCore.DataBase.Models
{
    [Table("UserProfiles")]
    public class UserProfile : IEquatable<UserProfile>
    {
        public UserProfile() { }

        public UserProfile(UserProfile userProfile)
        {
            Id = userProfile.Id;
            UserId = userProfile.UserId;
            Email = userProfile.Email;
            Name = userProfile.Name;
            Picture = userProfile.Picture;
            EmailNotificationsEnabled = userProfile.EmailNotificationsEnabled;
            PushNotificationsEnabled = userProfile.PushNotificationsEnabled;
            RememberAccount = userProfile.RememberAccount;
            SelectedAccountId = userProfile.SelectedAccountId;
            APIAccessToken = userProfile.APIAccessToken;
        }

        [Required]
        public string Id { get; set; }
        [JsonProperty("user_id")]
        public string UserId { get; set; }
        [Required]
        public string Email { get; set; }
        [JsonProperty("username")]
        public string Name { get; set; }
        public string Picture { get; set; }
        [Required]
        public bool EmailNotificationsEnabled { get; set; }
        [Required]
        public bool PushNotificationsEnabled { get; set; }
        [Required]
        public bool RememberAccount { get; set; }
        public string SelectedAccountId { get; set; }
        public string APIAccessToken { get; set; }

        public bool Equals(UserProfile other)
        {
            return Email == other?.Email && Name == other?.Name;
        }

        public bool IsValidated()
        {
            return !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(UserId);
        }

        public bool IsBuiltInAdmin()
        {
            return Name.Equals("admin", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
