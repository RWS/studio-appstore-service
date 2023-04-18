using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Model
{
    public class Change : IEquatable<Change>
    {
        public string Name { get; set; }
        public string New { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Old { get; set; }

        public bool Equals(Change other)
        {
            return Name == other?.Name &&
                   New == other?.New &&
                   Old == other?.Old;
        }
    }
}