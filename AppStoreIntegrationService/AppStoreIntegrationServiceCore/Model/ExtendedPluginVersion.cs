using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Reflection;

namespace AppStoreIntegrationServiceCore.Model
{
    public class ExtendedPluginVersion : PluginVersion<string>
    {
        public ExtendedPluginVersion() { }

        public ExtendedPluginVersion(PluginVersion<string> version)
        {
            PropertyInfo[] properties = typeof(PluginVersion<string>).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                property.SetValue(this, property.GetValue(version));
            }
        }

        [JsonIgnore]
        public int PluginId { get; set; }

        [JsonIgnore]
        [BindProperty]
        public MultiSelectList SupportedProductsListItems { get; set; }

        [JsonIgnore]
        public IEnumerable<ParentProduct> ParentProducts { get; set; }

        [JsonIgnore]
        [BindProperty]
        public string VersionName { get; set; }

        [JsonIgnore]
        public bool IsNewVersion { get; set; }

        [JsonIgnore]
        public IEnumerable<Comment> VersionComments { get; set; }

        public bool Equals(ExtendedPluginVersion other)
        {
            throw new NotImplementedException();
        }

        public void SetSupportedProductsList(List<ProductDetails> supportedProductDetails, List<ParentProduct> parents)
        {
            SupportedProductsListItems = new MultiSelectList
            (
                supportedProductDetails,
                nameof(ProductDetails.Id),
                nameof(ProductDetails.ProductName)
            );

            ParentProducts = parents;
        }
    }
}
