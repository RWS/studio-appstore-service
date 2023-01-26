using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace AppStoreIntegrationServiceCore.Model
{
    public class IconDetails : IEquatable<IconDetails>
    {
        [JsonProperty("MediaURL")]
        [Display(Name = "Icon url")]
        [Required(ErrorMessage = "Icon url is required!")]
        [RegularExpression(@"^https?:\/\/\w+([\.\-]\w+)*(:[0-9]+)?(\/.*)?$", ErrorMessage = "Invalid url!")]
        public string MediaUrl { get; set; }

        public bool Equals(IconDetails other)
        {
            return MediaUrl == other?.MediaUrl;
        }
    }
}
