using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginDetailsBase<T, U>
    {
        public int Id { get; set; }

        [Display(Name = "Plugin name")]
        [Required(ErrorMessage = "Plugin name is required!")]
        [RegularExpression(@"^(\w+[^<>=]?(\w+)?)(\s?\w+[^<>= ]?)*$", ErrorMessage = "Invalid plugin name!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description field is required!")]
        [MinLength(20, ErrorMessage = "The field description must contain at least 20 characters!")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Changelog link")]
        [Url(ErrorMessage = "Invalid url!")]
        public string ChangelogLink { get; set; }

        [Display(Name = "Support url")]
        [Url(ErrorMessage = "Invalid url!")]
        public string SupportUrl { get; set; }

        [Display(Name = "Support email")]
        [EmailAddress(ErrorMessage = "Invalid email address!")]
        public string SupportEmail { get; set; }
        public IconDetails Icon { get; set; } = new IconDetails();

        [Display(Name = "Paid for")]
        public bool PaidFor { get; set; }
        public DeveloperDetails Developer { get; set; } = new DeveloperDetails();
        public List<T> Versions { get; set; } = new List<T>();

        [Display(Name = "Plugin categories")]
        public List<U> Categories { get; set; } = new List<U>();

        [Required(ErrorMessage = "Plugin downoad url is required!")]
        [Url(ErrorMessage = "Invalid url!")]
        [Display(Name = "Download url")]
        public string DownloadUrl { get; set; }
        [Display(Name = "Status")]
        public Status Status { get; set; }

        public static PluginDetailsBase<T,U> CopyFrom(PluginDetails other)
        {
            return JsonConvert.DeserializeObject<PluginDetailsBase<T, U>>(JsonConvert.SerializeObject(other));
        }

        public bool Equals(PluginDetailsBase<T, U> other)
        {
            return Name == other.Name &&
                   Description == other.Description &&
                   ChangelogLink == other.ChangelogLink &&
                   SupportUrl == other.SupportUrl &&
                   SupportEmail == other.SupportEmail &&
                   Icon.Equals(other.Icon) &&
                   PaidFor == other.PaidFor &&
                   Developer.Equals(other.Developer) &&
                   Categories.SequenceEqual(other.Categories) &&
                   DownloadUrl == other.DownloadUrl &&
                   Status == other.Status;
        }
    }
}
