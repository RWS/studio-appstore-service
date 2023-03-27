using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Model
{
    public class Change
    {
        public string Name { get; set; }
        public string New { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Old { get; set; }
    }
}
