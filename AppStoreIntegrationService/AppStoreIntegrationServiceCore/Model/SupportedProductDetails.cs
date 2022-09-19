namespace AppStoreIntegrationServiceCore.Model
{
	public class SupportedProductDetails
	{
		public string Id { get; set; }
		public string ProductName { get; set; }
		public int? ParentProductID { get; set; }
		public string MinimumStudioVersion { get; set; }	
		public bool IsDefault { get; set; }
	}
}
