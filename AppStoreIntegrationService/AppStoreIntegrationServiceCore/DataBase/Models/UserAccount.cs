using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppStoreIntegrationServiceCore.DataBase.Models
{
    [Table("UserAccounts")]
    public class UserAccount : IEquatable<UserAccount>
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string UserProfileId { get; set; }
        [Required]
        public string AccountId { get; set; }
        [Required]
        public string UserRoleId { get; set; }

        public bool Equals(UserAccount other)
        {
            return UserProfileId == other?.UserProfileId &&
                   AccountId == other?.AccountId &&
                   UserRoleId == other?.UserRoleId;
        }

        public bool IsAssigned(UserAccount userAccount)
        {
            return UserProfileId == userAccount?.UserProfileId && AccountId == userAccount?.AccountId;
        }
    }
}
