using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppStoreIntegrationServiceCore.DataBase.Models
{
    [Table("UserProfiles")]
    public class UserProfile : IEquatable<UserProfile>
    {
        [Required]
        public string Id { get; set; }
        [JsonProperty("user_id")]
        public string UserId { get; set; }
        [Required]
        public string Email { get; set; }
        [JsonProperty("username")]
        public string Name { get; set; }
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
    }
}
