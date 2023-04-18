using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceCore.Model
{
    public class CategoryDetails : IEquatable<CategoryDetails>
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "Category name is required!")]
        public string Name { get; set; }

        public bool Equals(CategoryDetails other)
        {
            return Name == other?.Name;
        }

        public bool IsDuplicate(CategoryDetails other)
        {
            return Name == other?.Name && Id != other?.Id;
        }
    }
}