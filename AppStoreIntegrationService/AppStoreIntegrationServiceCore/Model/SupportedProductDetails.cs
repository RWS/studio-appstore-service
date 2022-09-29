using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Model
{
	public class SupportedProductDetails
	{
		public string Id { get; set; }
		public string ProductName { get; set; }
		public int? ParentProductID { get; set; }
		[JsonIgnore]
		public string MinimumStudioVersion { get; set; }	
	}
}
