namespace AppStoreIntegrationServiceCore.Model
{
    public class ProductDetails
    {
        public string Id { get; set; }
        public string ProductName { get; set; }
        public int? ParentProductID { get; set; }
        public string MinimumStudioVersion { get; set; }
        public bool IsDefault { get; set; }

        public bool IsValid()
        {
            return new[] { Id, ProductName, MinimumStudioVersion }.All(item => item != null);
        }
    }
}
