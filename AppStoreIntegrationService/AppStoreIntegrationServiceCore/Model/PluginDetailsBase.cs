using System;
using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginDetailsBase<T, U>
    {
        public PluginDetailsBase() { }

        public PluginDetailsBase(PluginDetails<PluginVersion<string>, string> other)
        {
            var thisProperties = typeof(PluginDetailsBase<T, U>).GetProperties();
            var otherProperties = typeof(PluginDetails<PluginVersion<string>, string>).GetProperties().Where(x => thisProperties.Any(y => y.Name.Equals(x.Name))).ToArray();

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

        [Required(ErrorMessage = "Plugin name is required!")]
        [RegularExpression(@"^(\(?[a-zA-Z]{1,}\)?)( ?(- ?)?\(?[a-zA-Z0-9]{1,}'?[).,]?)*$", ErrorMessage = "Invalid name!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description field is required!")]
        [MinLength(20, ErrorMessage = "The field description must contain at least 20 characters!")]
        public string Description { get; set; }

        [Url(ErrorMessage = "Invalid url!")]
        public string ChangelogLink { get; set; }

        [Url(ErrorMessage = "Invalid url!")]
        public string SupportUrl { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address!")]
        public string SupportEmail { get; set; }
        public IconDetails Icon { get; set; }
        public bool PaidFor { get; set; }
        public DeveloperDetails Developer { get; set; }
        public List<T> Versions { get; set; } = new List<T>();
        public List<U> Categories { get; set; } = new List<U>();
        [Required(ErrorMessage = "Plugin downoad url is required!")]
        [Url(ErrorMessage = "Invalid url!")]
        public string DownloadUrl { get; set; }
    }
}
