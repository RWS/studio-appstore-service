using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Model
{
    public class DeveloperDetails
    {
        public string DeveloperName { get; set; }
        public string DeveloperDescription { get; set; }
        [JsonProperty("DeveloperURL")]
        public string DeveloperUrl { get; set; }
    }
}
