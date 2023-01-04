using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceCore.Model
{
    public class ParentProduct : IEquatable<ParentProduct>
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "Parent product name is required!")]
        public string ProductName { get; set; }

        public bool Equals(ParentProduct other)
        {
            return ProductName == other.ProductName;
        }

        public bool IsValid()
        {
            return Id != null && ProductName != null;
        }
    }
}
