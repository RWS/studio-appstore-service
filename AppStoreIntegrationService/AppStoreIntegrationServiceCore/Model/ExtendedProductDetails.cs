using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Model
{
    public class ExtendedProductDetails : ProductDetails
    {
        [JsonIgnore]
        public MultiSelectList ParentProductsListItems { get; set; }
        public void SetParentProductsList(IEnumerable<ParentProduct> parents)
        {
            ParentProductsListItems = new MultiSelectList
            (
                parents,
                nameof(ParentProduct.Id),
                nameof(ParentProduct.ProductName)
            );
        }

        public static ExtendedProductDetails CopyFrom(ProductDetails other)
        {
            var toString = JsonConvert.SerializeObject(other);
            return JsonConvert.DeserializeObject<ExtendedProductDetails>(toString);
        }
    }
}
