using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceCore.Model
{
    public class DeveloperDetails
    {
        [RegularExpression(@"^(\(?[a-zA-Z]{1,}\)?)( ?(- ?)?\(?[a-zA-Z]{1,}'?[).,]?)*$", ErrorMessage = "Invalid name!")]
        public string DeveloperName { get; set; }
        public string DeveloperDescription { get; set; }
        [JsonProperty("DeveloperURL")]
        public string DeveloperUrl { get; set; }
    }
}
