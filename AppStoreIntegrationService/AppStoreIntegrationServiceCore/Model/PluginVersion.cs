using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginVersion
    {
        public PluginVersion() { }

        public PluginVersion(PluginVersion version)
        {
            PropertyInfo[] properties = typeof(PluginVersion).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                property.SetValue(this, property.GetValue(version));
            }
        }

        public DateTime? CreatedDate { get; set; }
        public int DownloadCount { get; set; }
        public string Id { get; set; }
        public DateTime? ReleasedDate { get; set; }
        public string TechnicalRequirements { get; set; }

        [Required]
        public string VersionNumber { get; set; }
        public string FileHash { get; set; }
        public List<SupportedProductDetails> SupportedProducts { get; set; }
        public bool AppHasStudioPluginInstaller { get; set; }


        //TODO: Create a new object for private plugin version
        /// <summary>
        /// For Studio 2021 is 16.0 by default
        /// </summary>

        [Required]
        public string MinimumRequiredVersionOfStudio { get; set; }

        [JsonProperty("SDLHosted")]
        public bool SdlHosted { get; set; }

        public bool IsNavigationLink { get; set; }

        [Required]
        public string DownloadUrl { get; set; }
        /// <summary>
        /// For the plugins from private repo (config file) by default will be set to true
        /// </summary>
        public bool IsPrivatePlugin { get; set; }
    }
}
