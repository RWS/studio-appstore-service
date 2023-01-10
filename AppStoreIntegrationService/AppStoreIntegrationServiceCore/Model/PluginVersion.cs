using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginVersion<T> : IEquatable<PluginVersion<T>>
    {
        private string _minimumRequiredVersionOfStudio;
        private string _maximumRequiredVersionOfStudio;
        public PluginVersion() { }

        public PluginVersion(PluginVersion<T> version)
        {
            PropertyInfo[] properties = typeof(PluginVersion<T>).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                property.SetValue(this, property.GetValue(version));
            }
        }

        [JsonProperty("Id")]
        public string VersionId { get; set; }
        [Required(ErrorMessage = "Version number is required!")]
        [RegularExpression(@"^(\d+\.)?(\d+\.)?(\d+\.)?(\d+)$", ErrorMessage = "Invalid version number!")]
        public string VersionNumber { get; set; }
        [Required(ErrorMessage = "Checksum is required!")]
        [RegularExpression(@"\b(?:[0-9a-z]-?){40}\b", ErrorMessage = "Invalid checksum!")]
        public string FileHash { get; set; }
        [Required(ErrorMessage = "At least one product is required!")]
        [MinLength(1)]
        public List<T> SupportedProducts { get; set; }
        public bool AppHasStudioPluginInstaller { get; set; }
        [RegularExpression(@"^(\d{1,2}\.)?(\d{1}\.)?(\d{1})$", ErrorMessage = "Invalid version number!")]
        public bool IsThirdParty { get; set; }
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

        public bool IsNavigationLink { get; set; }
        [Required(ErrorMessage = "Download url is required!")]
        [Url(ErrorMessage = "Invalid url!")]
        public string DownloadUrl { get; set; }
        public bool IsPrivatePlugin { get; set; }
        [JsonProperty("Status")]
        public Status VersionStatus { get; set; }

        public bool Equals(PluginVersion<T> other)
        {
            var properties = typeof(PluginVersion<string>).GetProperties().Where(p => !Equals(p.Name, "SupportedProducts"));
            foreach (PropertyInfo property in properties)
            {
                if (!Equals(property.GetValue(this), property.GetValue(other)))
                {
                    return false;
                }
            }

            return SupportedProducts.SequenceEqual(other.SupportedProducts);
        }
    }
}
