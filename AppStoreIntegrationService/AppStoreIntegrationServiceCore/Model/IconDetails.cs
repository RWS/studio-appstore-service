using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceCore.Model
{
    public class IconDetails
    {
        [JsonProperty("MediaURL")]
        [Required(ErrorMessage = "Icon url is required!")]
        [RegularExpression(@"^https?:\/\/\w+([\.\-]\w+)*(:[0-9]+)?(\/.*)?$", ErrorMessage = "Invalid url!")]
        public string MediaUrl { get; set; }
    }
}
