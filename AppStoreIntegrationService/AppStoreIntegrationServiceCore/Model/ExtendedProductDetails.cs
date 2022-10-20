using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json.Serialization;

namespace AppStoreIntegrationServiceCore.Model
{
    public class ExtendedProductDetails : ProductDetails
    {
        public ExtendedProductDetails() { }
        public ExtendedProductDetails(ProductDetails product) : base(product) { }
        [JsonIgnore]
        public SelectList ParentProductsListItems { get; set; }
        public void SetParentProductsList(List<ParentProduct> parents)
        {
            ParentProductsListItems = new SelectList
            (
                parents,
                nameof(ParentProduct.ParentId),
                nameof(ParentProduct.ParentProductName),
                parents.FirstOrDefault(p => p.ParentId.Equals(ParentProductID))?.ParentId
            );
        }

    }
}
