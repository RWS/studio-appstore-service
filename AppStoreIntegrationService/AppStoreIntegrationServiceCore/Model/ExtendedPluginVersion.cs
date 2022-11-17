using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceCore.Model
{
    public class ExtendedPluginVersion<T> : PluginVersion<T>
    {
        private string versionDownloadUrl;
        public ExtendedPluginVersion() { }

        public ExtendedPluginVersion(PluginVersion<T> version) : base(version)
        {
            VersionDownloadUrl = version.DownloadUrl;
        }

        [Required(ErrorMessage = "Download url is required!")]
        [Url(ErrorMessage = "Invalid url!")]
        [JsonIgnore]
        public string VersionDownloadUrl
        {
            get => versionDownloadUrl;
            set
            {
                versionDownloadUrl = value;
                DownloadUrl = value;
            }
        }

        [Required(ErrorMessage = "At least one product is required!")]
        [MinLength(1)]
        [JsonIgnore]
        public List<string> SelectedProductIds { get; set; }

        [JsonIgnore]
        [BindProperty]
        public ProductDetails SelectedProduct { get; set; }

        [JsonIgnore]
        [BindProperty]
        public MultiSelectList SupportedProductsListItems { get; set; }

        [JsonIgnore]
        [BindProperty]
        public string VersionName { get; set; }

        [JsonIgnore]
        public bool IsNewVersion { get; set; }

        public void SetSupportedProductsList(List<ProductDetails> supportedProductDetails, string defaultProduct)
        {
            SupportedProductsListItems = new MultiSelectList
            (
                supportedProductDetails,
                nameof(ProductDetails.Id),
                nameof(ProductDetails.ProductName),
                defaultProduct
            );
        }
    }
}
