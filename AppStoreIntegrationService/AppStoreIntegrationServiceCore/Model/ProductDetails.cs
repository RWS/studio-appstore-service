using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AppStoreIntegrationServiceCore.Model
{
    public class ProductDetails : IEquatable<ProductDetails>
    {
        private string _minimumStudioVersion;

        public ProductDetails() { }
        public ProductDetails(ProductDetails other)
        {
            PropertyInfo[] properties = typeof(ProductDetails).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                property.SetValue(this, property.GetValue(other));
            }
        }

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
            return ProductName.Equals(other.ProductName) && MinimumStudioVersion.Equals(other.MinimumStudioVersion);
        }
    }
}
