using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginVersion
    {
        private readonly List<SupportedProductDetails> _supportedProductDetails;
        private SupportedProductDetails _selectedProduct;

        public PluginVersion() { }

        public PluginVersion(List<SupportedProductDetails> supportedProductDetails)
        {
            _supportedProductDetails = supportedProductDetails;
            SupportedProductsListItems = new SelectList
            (
                supportedProductDetails,
                nameof(SupportedProductDetails.Id),
                nameof(SupportedProductDetails.ProductName),
                supportedProductDetails.FirstOrDefault(x => x.IsDefault)?.Id
            );
        }

        public DateTime? CreatedDate { get; set; }
        public int DownloadCount { get; set; }
        public string Id { get; set; }
        public DateTime? ReleasedDate { get; set; }
        public string TechnicalRequirements { get; set; }

        [Required]
        public string VersionNumber { get; set; }
        public string FileHash { get; set; }
        public List<SupportedProductDetails> SupportedProducts { get; set; }
        public bool AppHasStudioPluginInstaller { get; set; }


        //TODO: Create a new object for private plugin version
        /// <summary>
        /// For Studio 2021 is 16.0 by default
        /// </summary>

        [Required]
        public string MinimumRequiredVersionOfStudio { get; set; }

        [JsonProperty("SDLHosted")]
        public bool SdlHosted { get; set; }

        public bool IsNavigationLink { get; set; }

        [Required]
        public string DownloadUrl { get; set; }
        /// <summary>
        /// For the plugins from private repo (config file) by default will be set to true
        /// </summary>
        public bool IsPrivatePlugin { get; set; }

        // Properties used in Config Tool app	
        [JsonIgnore]
        [BindProperty]
        public string SelectedProductId { get; set; }

        [JsonIgnore]
        [BindProperty]
        public SupportedProductDetails SelectedProduct
        {
            get => _selectedProduct ?? _supportedProductDetails?.Last();
            set
            {
                _selectedProduct = value;
                UpdateStudioMinVersion();
            }
        }

        [JsonIgnore]
        [BindProperty]
        public SelectList SupportedProductsListItems { get; set; }

        [JsonIgnore]
        [BindProperty]
        public string VersionName { get; set; }
        [JsonIgnore]
        public bool IsNewVersion { get; set; }

        public void SetSupportedProducts()
        {
            if (SupportedProducts == null)
            {
                SupportedProducts = new List<SupportedProductDetails>();
                SupportedProducts.AddRange(_supportedProductDetails);
            }
        }

        private void UpdateStudioMinVersion()
        {
            if (string.IsNullOrEmpty(MinimumRequiredVersionOfStudio))
            {
                if (string.IsNullOrEmpty(SelectedProduct?.MinimumStudioVersion))
                {
                    var productDetails = _supportedProductDetails.FirstOrDefault(v => v.ProductName.Equals(SelectedProduct.ProductName));
                    if (productDetails != null)
                    {
                        var minVersion = productDetails.MinimumStudioVersion;
                        SelectedProduct.MinimumStudioVersion = minVersion;
                        MinimumRequiredVersionOfStudio = minVersion;
                    }
                }
            }
        }
    }
}
