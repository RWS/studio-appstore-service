using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppStoreIntegrationServiceCore.DataBase.Models
{
    [Table("AccountAgreements")]
    public class AccountAgreement
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string AccountId { get; set; }
        [Required]
        public string UserProfileId { get; set; }
        [Required]
        public string Name { get; set; } = "Technology partener agreement";
        [Required]
        public string Version { get; set; } = "1.0";
        [Required]
        public DateTime DateTime { get; set; } = DateTime.Now;
    }
}
