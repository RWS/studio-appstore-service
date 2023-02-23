using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppStoreIntegrationServiceManagement.Model.DataBase
{
    [Table("AspNetUserAccounts")]
    public class UserAccount
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string AccountId { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string RoleId { get; set; }
        [Required]
        public bool IsOwner { get; set; }
    }
}
