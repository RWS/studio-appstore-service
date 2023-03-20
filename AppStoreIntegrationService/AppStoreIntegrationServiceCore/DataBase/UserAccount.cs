using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppStoreIntegrationServiceCore.DataBase
{
    [Table("AspNetUserAccounts")]
    public class UserAccount
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string AccountId { get; set; }
        [Required]
        public string ParentAccountId { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string RoleId { get; set; }
        [Required]
        public bool IsOwner { get; set; }

        public bool IsAssigned(UserAccount userAccount)
        {
            return UserId == userAccount.UserId &&
                   AccountId == userAccount.AccountId &&
                   ParentAccountId == userAccount.ParentAccountId;
        }

        public bool IsInRole(IdentityUserExtended user, string roleId)
        {
            return ParentAccountId == user.SelectedAccountId &&
                   UserId == user.Id &&
                   RoleId == roleId;
        }

        public bool HasFullOwnership(IdentityUserExtended user, string roleId)
        {
            return ParentAccountId == user.SelectedAccountId &&
                   UserId == user.Id &&
                   RoleId == roleId &&
                   IsOwner;
        }
    }
}
