using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginVersion<T>
    {
        public PluginVersion() { }

        public PluginVersion(PluginVersion<T> version)
        {
            PropertyInfo[] properties = typeof(PluginVersion<T>).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                property.SetValue(this, property.GetValue(version));
            }
        }

        public DateTime? CreatedDate { get; set; }
        public int DownloadCount { get; set; }
        [JsonProperty("Id")]
        public string VersionId { get; set; }
        public DateTime? ReleasedDate { get; set; }
        public string TechnicalRequirements { get; set; }
        [Required(ErrorMessage = "Version number is required!")]
        [RegularExpression(@"^(\d+\.)?(\d+\.)?(\d+\.)?(\d+)$", ErrorMessage = "Invalid version number!")]
        public string VersionNumber { get; set; }
        [Required(ErrorMessage = "Checksum is required!")]
        [RegularExpression(@"\b(?:[0-9a-z]-?){40}\b", ErrorMessage = "Invalid checksum!")]
        public string FileHash { get; set; }
        public List<T> SupportedProducts { get; set; }
        public bool AppHasStudioPluginInstaller { get; set; }
        [RegularExpression(@"^(\d{1,2}\.)?(\d{1})$", ErrorMessage = "Invalid version number!")]
        public string MinimumRequiredVersionOfStudio { get; set; }
        [RegularExpression(@"^(\d{1,2}\.)?(\d{1})$", ErrorMessage = "Invalid version number!")]
        public string MaximumRequiredVersionOfStudio { get; set; }
        [JsonProperty("SDLHosted")]
        public bool SdlHosted { get; set; }
        public bool IsNavigationLink { get; set; }
        [Required(ErrorMessage = "Version download url is required!")]
        [JsonProperty("DownloadUrl")]
        [Url(ErrorMessage = "Invalid url!")]
        public string VersionDownloadUrl { get; set; }
        public bool IsPrivatePlugin { get; set; }
    }
}
