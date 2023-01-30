using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceCore.Model
{
    public class NameMapping : IEquatable<NameMapping>
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "New name is required!")]
        public string NewName { get; set; }

        [Required(ErrorMessage = "Old name is required!")]
        public string OldName { get; set; }

        public bool Equals(NameMapping other)
        {
            return NewName == other?.NewName &&
                   OldName == other?.OldName;
        }

        public bool IsDuplicate(NameMapping other)
        {
            return NewName == other.NewName &&
                   OldName == other.OldName &&
                   Id != other.Id;
        }
    }
}
