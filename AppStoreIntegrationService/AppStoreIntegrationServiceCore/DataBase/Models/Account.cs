using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppStoreIntegrationServiceCore.DataBase.Models
{
    [Table("Accounts")]
    public class Account
    {
        [Required]
        public string Id { get; set; }
        public string SalesForceId { get; set; }
        public string OosId { get; set; }
        public string Name { get; set; }
    }
}
