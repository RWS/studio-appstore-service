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
        [Required]
        public string VersionNumber { get; set; }
        public string FileHash { get; set; }
        public List<T> SupportedProducts { get; set; }
        public bool AppHasStudioPluginInstaller { get; set; }
        public string MinimumRequiredVersionOfStudio { get; set; }
        [JsonProperty("SDLHosted")]
        public bool SdlHosted { get; set; }
        public bool IsNavigationLink { get; set; }

        [Required]
        [JsonProperty("DownloadUrl")]
        [Url(ErrorMessage = "DownloadUrl is in wrong format!")]
        public string VersionDownloadUrl { get; set; }
        public bool IsPrivatePlugin { get; set; }
    }
}
