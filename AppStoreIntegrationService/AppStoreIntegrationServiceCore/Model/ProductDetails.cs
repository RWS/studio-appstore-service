using System.Reflection;
using System.Text.RegularExpressions;

namespace AppStoreIntegrationServiceCore.Model
{
    public class ProductDetails
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
        public string ProductName { get; set; }
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

        public bool IsValid()
        {
            return new[] { Id, ProductName, MinimumStudioVersion }.All(item => item != null);
        }
    }
}
