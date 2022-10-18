using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Model
{
    public class ExtendedPluginVersion<T> : PluginVersion<T>
    {
        private ProductDetails _selectedProduct;

        public ExtendedPluginVersion() { }

        public ExtendedPluginVersion(PluginVersion<T> version) : base(version) { }

        [JsonIgnore]
        [BindProperty]
        public string SelectedProductId { get; set; }

        [JsonIgnore]
        [BindProperty]
        public ProductDetails SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                //UpdateStudioMinVersion();
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

        /*private void UpdateStudioMinVersion()
        {
            if (string.IsNullOrEmpty(MinimumRequiredVersionOfStudio))
            {
                if (string.IsNullOrEmpty(SelectedProduct?.MinimumStudioVersion))
                {
                    var productDetails = SupportedProducts?.FirstOrDefault(v => v.ProductName.Equals(SelectedProduct?.ProductName));
                    if (productDetails != null)
                    {
                        var minVersion = productDetails.MinimumStudioVersion;
                        SelectedProduct.MinimumStudioVersion = minVersion;
                        MinimumRequiredVersionOfStudio = minVersion;
                    }
                }
            }
        }*/
    }
}
