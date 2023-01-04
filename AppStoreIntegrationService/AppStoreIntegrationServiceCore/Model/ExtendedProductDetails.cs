using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json.Serialization;

namespace AppStoreIntegrationServiceCore.Model
{
    public class ExtendedProductDetails : ProductDetails
    {
        public ExtendedProductDetails() { }
        public ExtendedProductDetails(ProductDetails product) : base(product) { }
        [JsonIgnore]
        public MultiSelectList ParentProductsListItems { get; set; }
        public void SetParentProductsList(List<ParentProduct> parents)
        {
            ParentProductsListItems = new MultiSelectList
            (
                parents,
                nameof(ParentProduct.Id),
                nameof(ParentProduct.ProductName)
            );
        }

    }
}
