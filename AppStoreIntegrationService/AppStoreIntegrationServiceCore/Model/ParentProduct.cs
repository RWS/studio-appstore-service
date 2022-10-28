using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Model
{
    public class ParentProduct : IEquatable<ParentProduct>
    {
        [JsonProperty("Id")]
        public string ParentId { get; set; }
        [JsonProperty("ProductName")]
        public string ParentProductName { get; set; }

        public bool Equals(ParentProduct other)
        {
            return ParentProductName == other.ParentProductName;
        }

        public bool IsValid()
        {
            return ParentId != null && ParentProductName != null;
        }
    }
}
