using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AppStoreIntegrationServiceCore.Model
{
    public class ProductDetails : IEquatable<ProductDetails>
    {
        private string _minimumStudioVersion;

        public string Id { get; set; }
        [Required(ErrorMessage = "The product name is required!")]
        public string ProductName { get; set; }
        [Required(ErrorMessage = "Minimum version is required!")]
        public string MinimumStudioVersion
        {
            get 
            {
                if (_minimumStudioVersion != null && Regex.IsMatch(_minimumStudioVersion, @"^(\d{1,2}\.)?(\d{1})$"))
                {
                    return $"{_minimumStudioVersion}.0";
                }

                return _minimumStudioVersion;
            }
            set 
            { 
                _minimumStudioVersion = value;
            }
        }
        public string ParentProductID { get; set; }
        public bool IsLegacy { get; set; }

        public bool Equals(ProductDetails other)
        {
            return ProductName == other.ProductName && MinimumStudioVersion == other.MinimumStudioVersion;
        }
    }
}
