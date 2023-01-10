using System.ComponentModel.DataAnnotations;
using System.Reflection;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginDetails<T, U> : IEquatable<PluginDetails<T, U>>
    {
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
        public Status Status { get; set; }
        public DeveloperDetails Developer { get; set; }
        public List<T> Versions { get; set; } = new List<T>();
        public List<U> Categories { get; set; } = new List<U>();
        public bool IsThirdParty { get; set; }
        [Required(ErrorMessage = "Plugin downoad url is required!")]
        [Url(ErrorMessage = "Invalid url!")]
        public string DownloadUrl { get; set; }

        public bool Equals(PluginDetails<T, U> other)
        {
            var properties = typeof(PluginDetails<T, U>).GetProperties().Where(p => !p.Name.Equals("Versions"));

            foreach (PropertyInfo property in properties)
            {
                bool ok = property.Name switch
                {
                    "Categories" => Categories.SequenceEqual(other.Categories),
                    "Icon" => Icon.Equals(other.Icon),
                    "Developer" => Developer.Equals(other.Developer),
                    _ => Equals(property.GetValue(this), property.GetValue(other))
                };

                if (!ok)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
