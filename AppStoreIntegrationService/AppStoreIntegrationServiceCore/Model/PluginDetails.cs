using AppStoreIntegrationServiceCore.Comparers;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginDetails<T, U> : IEquatable<PluginDetails<T, U>>
    {
        public PluginDetails() { }

        public PluginDetails(ExtendedPluginDetails other)
        {
            var thisProperties = typeof(PluginDetails<T, U>).GetProperties();
            var otherProperties = typeof(PluginDetails<ExtendedPluginVersion, string>).GetProperties();

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
        public DateTime? ReleaseDate { get; set; }
        public int DownloadCount { get; set; }
        public int CommentCount { get; set; }
        public string SupportText { get; set; }
        public bool PaidFor { get; set; }
        public Status Status { get; set; }
        public string Pricing { get; set; }
        public RatingDetails RatingSummary { get; set; }
        public DeveloperDetails Developer { get; set; }
        public List<IconDetails> Media { get; set; }
        public List<T> Versions { get; set; } = new List<T>();
        public List<U> Categories { get; set; } = new List<U>();

        [Required(ErrorMessage = "Plugin downoad url is required!")]
        [Url(ErrorMessage = "Invalid url!")]
        public string DownloadUrl { get; set; }
        public DateTime? CreatedDate { get; set; }

        public bool Equals(PluginDetails<T, U> other)
        {
            var properties = typeof(PluginDetails<T, U>).GetProperties().Where(p => !p.Name.Equals("Versions"));

            foreach (PropertyInfo property in properties)
            {
                bool ok = property.Name switch
                {
                    "Categories" => Categories.SequenceEqual(other.Categories),
                    "Icon" => Icon.Equals(other.Icon),
                    "RatingSummary" => RatingSummary?.Equals(other.RatingSummary) ?? true,
                    "Developer" => Developer.Equals(other.Developer),
                    "Media" => Media?.SequenceEqual(other.Media, new IconEqualityComparer()) ?? true,
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
