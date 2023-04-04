using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceCore.Model
{
    public class DeveloperDetails : IEquatable<DeveloperDetails>
    {
        [Display(Name = "Developer name")]
        public string DeveloperName { get; set; }
        public string DeveloperDescription { get; set; }
        [JsonProperty("DeveloperURL")]
        public string DeveloperUrl { get; set; }

        public bool Equals(DeveloperDetails other)
        {
            return DeveloperName == other?.DeveloperName &&
                   DeveloperDescription == other?.DeveloperDescription &&
                   DeveloperUrl == other?.DeveloperUrl;
        }
    }
}
