using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Reflection;
using System;

namespace AppStoreIntegrationServiceCore.Model
{
    public class ProductDetails
    {
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
        public string MinimumStudioVersion { get; set; }
        public bool IsDefault { get; set; }
        public string ParentProductID { get; set; }

        public bool IsValid()
        {
            return new[] { Id, ProductName, MinimumStudioVersion }.All(item => item != null);
        }
    }
}
