using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppStoreIntegrationServiceCore.DataBase.Models
{
    [Table("UserRoles")]
    public class UserRole : IEquatable<UserRole>
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }

        public bool Equals(UserRole other)
        {
            return Name == other?.Name;
        }
    }
}
