using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Model
{
    public class ExtendedPluginDetails<T> where T : PluginVersion<string>
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Plugin name is required!")]
        [RegularExpression(@"^(\(?[a-zA-Z]{1,}\)?)( ?(- ?)?\(?[a-zA-Z0-9]{1,}'?[).,]?)*$", ErrorMessage = "Invalid name!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description field is required!")]
        [MinLength(20, ErrorMessage = "The field description must contain at least 20 characters!")]
        public string Description { get; set; }

        public bool PaidFor { get; set; }

        [Url(ErrorMessage = "Invalid url!")]
        public string ChangelogLink { get; set; }

        [Url(ErrorMessage = "Invalid url!")]
        public string SupportUrl { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address!")]
        public string SupportEmail { get; set; }

        public Status Status { get; set; }

        [Required(ErrorMessage = "Plugin downoad url is required!")]
        [Url(ErrorMessage = "Invalid url!")]
        public string DownloadUrl { get; set; }

        public List<string> Categories { get; set; } = new List<string>();

        public List<ExtendedPluginVersion<string>> Versions { get; set; }

        [Required(ErrorMessage = "Icon url is required!")]
        [RegularExpression(@"^https?:\/\/\w+([\.\-]\w+)*(:[0-9]+)?(\/.*)?$", ErrorMessage = "Invalid url!")]
        public string IconUrl { get; set; }

        [RegularExpression(@"^(\(?[a-zA-Z]{1,}\)?)( ?(- ?)?\(?[a-zA-Z]{1,}'?[).,]?)*$", ErrorMessage = "Invalid name!")]
        public string DeveloperName { get; set; }

        public bool IsEditMode { get; set; }

        public MultiSelectList CategoryListItems { get; set; }

        public string SelectedVersionId { get; set; }

        public bool IsValid(PluginVersion<string> selectedVersion)
        {
            return selectedVersion.VersionId != null || IsEditMode;
        }

        public void SetVersionList(List<ExtendedPluginVersion<string>> versions, ExtendedPluginVersion<string> selectedVersion)
        {
            var editedVersion = versions.FirstOrDefault(v => v.VersionId.Equals(selectedVersion.VersionId));
            if (editedVersion != null)
            {
                versions[versions.IndexOf(editedVersion)] = selectedVersion;
            }
            else if (selectedVersion.VersionId != null)
            {
                versions.Add(selectedVersion);
            }

            Versions = versions;
        }

        public void SetDownloadUrl()
        {
            DownloadUrl = Versions.LastOrDefault()?.VersionDownloadUrl;
        }
    }

}

