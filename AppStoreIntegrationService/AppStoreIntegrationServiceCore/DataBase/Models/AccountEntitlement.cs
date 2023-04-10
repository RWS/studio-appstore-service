using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppStoreIntegrationServiceCore.DataBase.Models
{
    [Table("AccountEntitlements")]
    public class AccountEntitlement : IEquatable<AccountEntitlement>
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string AccountId { get; set; }
        [Required]
        public string Name { get; set; } = "AppStore Manager";

        public bool Equals(AccountEntitlement other)
        {
            return Id == other?.Id &&
                   AccountId == other?.AccountId &&
                   Name == other?.Name;
        }
    }
}
