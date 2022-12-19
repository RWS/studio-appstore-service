using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
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

        [Required(ErrorMessage = "Download url is required!")]
        [Url(ErrorMessage = "Invalid url!")]
        [JsonIgnore]
        public string VersionDownloadUrl 
        { 
            get => DownloadUrl; 
            set
            {
                DownloadUrl = value;
            }
        }

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
