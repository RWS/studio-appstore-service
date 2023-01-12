using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceCore.Model
{
    public class DeveloperDetails : IEquatable<DeveloperDetails>
    {
        [RegularExpression(@"^(\(?[a-zA-Z]{1,}\)?)( ?(- ?)?\(?[a-zA-Z]{1,}'?[).,]?)*$", ErrorMessage = "Invalid name!")]
        [Display(Name = "Developer name")]
        public string DeveloperName { get; set; }
        public string DeveloperDescription { get; set; }
        [JsonProperty("DeveloperURL")]
        public string DeveloperUrl { get; set; }

        public bool Equals(DeveloperDetails other)
        {
            return Equals(DeveloperName, other.DeveloperName) &&
                   Equals(DeveloperDescription, other.DeveloperDescription) &&
                   Equals(DeveloperUrl, other.DeveloperUrl);
        }
    }
}
