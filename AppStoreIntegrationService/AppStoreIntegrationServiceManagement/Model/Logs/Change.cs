using Newtonsoft.Json;

namespace AppStoreIntegrationServiceManagement.Model.Logs
{
    public class Change
    {
        public string Name { get; set; }
        public string New { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Old { get; set; }
    }
}
