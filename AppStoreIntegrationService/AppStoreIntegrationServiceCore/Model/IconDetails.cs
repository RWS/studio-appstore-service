using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceCore.Model
{
    public class IconDetails : IEquatable<IconDetails>
    {
        [JsonProperty("MediaURL")]
        [Required(ErrorMessage = "Icon url is required!")]
        [RegularExpression(@"^https?:\/\/\w+([\.\-]\w+)*(:[0-9]+)?(\/.*)?$", ErrorMessage = "Invalid url!")]
        public string MediaUrl { get; set; }

        public bool Equals(IconDetails other)
        {
            return Equals(MediaUrl, other.MediaUrl);
        }
    }
}
