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
            VersionDownloadUrl= version.DownloadUrl;
        }

        [Required(ErrorMessage = "Version download url is required!")]
        [JsonProperty("DownloadUrl")]
        [Url(ErrorMessage = "Invalid url!")]
        public string VersionDownloadUrl
        {
            get => versionDownloadUrl;
            set
            {
                versionDownloadUrl = value;
                DownloadUrl = value;
            }
        }

        [JsonIgnore]
        [BindProperty]
        public string SelectedProductId { get; set; }

        [JsonIgnore]
        [BindProperty]
        public ProductDetails SelectedProduct { get; set; }

        [JsonIgnore]
        [BindProperty]
        public SelectList SupportedProductsListItems { get; set; }

        [JsonIgnore]
        [BindProperty]
        public string VersionName { get; set; }

        [JsonIgnore]
        public bool IsNewVersion { get; set; }

        [Required(ErrorMessage = "Manifest is required!")]
        [JsonIgnore]
        public ImportManifestModel ManifestFile { get; set; }

        public void SetSupportedProductsList(List<ProductDetails> supportedProductDetails, string defaultProduct)
        {
            SupportedProductsListItems = new SelectList
            (
                supportedProductDetails,
                nameof(ProductDetails.Id),
                nameof(ProductDetails.ProductName),
                defaultProduct
            );
        }
    }
}
