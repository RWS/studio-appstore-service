using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppStoreIntegrationServiceManagement.Model.DataBase
{
    [Table("AspNetAccounts")]
    public class Account
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string AccountName { get; set; }
        [Required]
        public bool IsAppStoreAccount { get; set; }
    }
}
