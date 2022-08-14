using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Model
{
	public class IconDetails
	{
		[JsonProperty("MediaURL")]
		public string MediaUrl { get; set; }
	}
}
