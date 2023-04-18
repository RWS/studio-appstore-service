using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppStoreIntegrationServiceCore.DataBase.Models
{
    [Table("Accounts")]
    public class Account : IEquatable<Account>
    {
        public Account() { }

        public Account(Account account)
        {
            Id = account.Id;
            SalesForceId = account.SalesForceId;
            OosId = account.OosId;
            Name = account.Name;
        }

        [Required]
        public string Id { get; set; }
        public string SalesForceId { get; set; }
        public string OosId { get; set; }
        public string Name { get; set; }

        public bool Equals(Account other)
        {
            return Name == other?.Name;
        }
    }
}