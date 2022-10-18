using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Model
{
    public class ProductDetails
    {
        public string Id { get; set; }
        public string ProductName { get; set; }
        public string MinimumStudioVersion { get; set; }
        public bool IsDefault { get; set; }
        public string ParentProductID { get; set; }

        [JsonIgnore]
        public SelectList ParentProductsListItems { get; set; }

        public bool IsValid()
        {
            return new[] { Id, ProductName, MinimumStudioVersion }.All(item => item != null);
        }

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
