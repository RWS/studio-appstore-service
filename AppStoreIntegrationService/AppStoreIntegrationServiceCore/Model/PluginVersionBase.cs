using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginVersionBase<T>
    {
        private string _minimumRequiredVersionOfStudio;
        private string _maximumRequiredVersionOfStudio;

        [JsonProperty("Id")]
        public string VersionId { get; set; }

        [Display(Name = "Version number")]
        [Required(ErrorMessage = "Version number is required!")]
        [RegularExpression(@"^(\d+\.)?(\d+\.)?(\d+\.)?(\d+)$", ErrorMessage = "Invalid version number!")]
        public string VersionNumber { get; set; }
        [Display(Name = "Checksum")]
        [Required(ErrorMessage = "Checksum is required!")]
        [RegularExpression(@"\b(?:[0-9a-z]-?){40}\b", ErrorMessage = "Invalid checksum!")]
        public string FileHash { get; set; }

        [Required(ErrorMessage = "At least one product is required!")]
        [MinLength(1)]
        public List<T> SupportedProducts { get; set; }

        [Display(Name = "App Has Studio Plugin Installer")]
        public bool AppHasStudioPluginInstaller { get; set; }

        [Required]
        [Display(Name = "Minimum Required Studio Version")]
        [RegularExpression(@"^(\d{1,2}\.)?(\d{1}\.)?(\d{1})$", ErrorMessage = "Invalid version number!")]
        public string MinimumRequiredVersionOfStudio
        {
            get
            {
                if (_minimumRequiredVersionOfStudio != null && Regex.IsMatch(_minimumRequiredVersionOfStudio, @"^(\d{1,2}\.)?(\d{1})$"))
                {
                    return $"{_minimumRequiredVersionOfStudio}.0";
                }

                return _minimumRequiredVersionOfStudio;
            }
            set
            {
                _minimumRequiredVersionOfStudio = value;
            }
        }

        [Required]
        [Display(Name = "Maximum Required Studio Version")]
        [RegularExpression(@"^(\d{1,2}\.)?(\d{1}\.)?(\d{1})$", ErrorMessage = "Invalid version number!")]
        public string MaximumRequiredVersionOfStudio
        {
            get
            {
                if (_maximumRequiredVersionOfStudio != null && Regex.IsMatch(_maximumRequiredVersionOfStudio, @"^(\d{1,2}\.)?(\d{1})$"))
                {
                    return $"{_maximumRequiredVersionOfStudio}.0";
                }

                return _maximumRequiredVersionOfStudio;
            }
            set
            {
                _maximumRequiredVersionOfStudio = value;
            }
        }

        [Display(Name = "Is Navigation Link ")]
        public bool IsNavigationLink { get; set; }

        [Display(Name = "Download Url")]
        [Required(ErrorMessage = "Download url is required!")]
        [Url(ErrorMessage = "Invalid url!")]
        public string DownloadUrl { get; set; }
        public bool IsPrivatePlugin { get; set; }
        [Display(Name = "Status")]
        [JsonProperty("Status")]
        public Status VersionStatus { get; set; }

        public bool Equals(PluginVersionBase<T> other)
        {
            return VersionNumber == other?.VersionNumber &&
                   FileHash == other?.FileHash &&
                   SupportedProducts.SequenceEqual(other.SupportedProducts) &&
                   AppHasStudioPluginInstaller == other?.AppHasStudioPluginInstaller &&
                   MinimumRequiredVersionOfStudio == other?.MinimumRequiredVersionOfStudio &&
                   MaximumRequiredVersionOfStudio == other?.MaximumRequiredVersionOfStudio &&
                   IsNavigationLink == other?.IsNavigationLink &&
                   DownloadUrl == other?.DownloadUrl &&
                   IsPrivatePlugin == other?.IsPrivatePlugin &&
                   VersionStatus == other?.VersionStatus;
        }

        public static PluginVersionBase<T> CopyFrom(PluginVersion other)
        {
            if (other == null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<PluginVersionBase<T>>(JsonConvert.SerializeObject(other));
        }
    }
}
