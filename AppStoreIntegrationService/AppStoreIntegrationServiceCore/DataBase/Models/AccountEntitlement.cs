using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppStoreIntegrationServiceCore.DataBase.Models
{
    [Table("AccountEntitlements")]
    public class AccountEntitlement
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string AccountId { get; set; }
        [Required]
        public string Name { get; set; } = "AppStore Manager";
    }
}
