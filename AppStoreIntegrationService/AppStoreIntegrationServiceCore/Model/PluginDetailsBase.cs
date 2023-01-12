using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginDetailsBase<T, U>
    {
        public PluginDetailsBase() { }

        public PluginDetailsBase(PluginDetails other)
        {
            var thisProperties = typeof(PluginDetailsBase<T, U>).GetProperties();
            var otherProperties = typeof(PluginDetails).GetProperties().Where(x => thisProperties.Any(y => y.Name.Equals(x.Name))).ToArray();

            for (var i = 0; i < thisProperties.Length; i++)
            {
                if (thisProperties[i].Name != "Versions")
                {
                    thisProperties[i].SetValue(this, otherProperties[i].GetValue(other));
                }
            }

            Versions = other.Versions?.Cast<T>().ToList();
        }
        public int Id { get; set; }

        [Display(Name = "Plugin name")]
        [Required(ErrorMessage = "Plugin name is required!")]
        [RegularExpression(@"^(\(?[a-zA-Z]{1,}\)?)( ?(- ?)?\(?[a-zA-Z0-9]{1,}'?[).,]?)*$", ErrorMessage = "Invalid name!")]
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
        public IconDetails Icon { get; set; }

        [Display(Name = "Paid for")]
        public bool PaidFor { get; set; }
        public DeveloperDetails Developer { get; set; }
        public List<T> Versions { get; set; } = new List<T>();
        [Display(Name = "Plugin categories")]
        public List<U> Categories { get; set; } = new List<U>();
        [Required(ErrorMessage = "Plugin downoad url is required!")]
        [Url(ErrorMessage = "Invalid url!")]
        [Display(Name = "Download url")]
        public string DownloadUrl { get; set; }
    }
}
