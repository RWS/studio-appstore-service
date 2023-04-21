using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Model
{
    public class Change : IEquatable<Change>
    {
        private string _new;
        private string _old;

        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string New 
        {
            get => _new == _old ? null : _old;
            set => _new = value;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Old
        {
            get => _old == _new ? null : _new;
            set => _old = value;
        }

        public bool Equals(Change other)
        {
            return Name == other?.Name &&
                   New == other?.New &&
                   Old == other?.Old;
        }
    }
}