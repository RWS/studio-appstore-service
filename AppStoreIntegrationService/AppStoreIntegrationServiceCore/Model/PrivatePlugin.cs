using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceCore.Model
{
    public class PrivatePlugin<T> where T : PluginVersion<string>
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [MinLength(20)]
        public string Description { get; set; }

        public bool PaidFor { get; set; }

        [Url(ErrorMessage = "ChangelogLink is in wrong format!")]
        public string ChangelogLink { get; set; }

        [Url(ErrorMessage = "SupportUrl is in wrong format!")]
        public string SupportUrl { get; set; }

        [EmailAddress(ErrorMessage = "SupportEmail is in wrong format!")]
        public string SupportEmail { get; set; }

        public bool Inactive { get; set; }

        [Required]
        [Url(ErrorMessage = "DownloadUrl is in wrong format!")]
        [MinLength(5)]
        public string DownloadUrl { get; set; }

        public List<string> Categories { get; set; }

        public List<ExtendedPluginVersion<string>> Versions { get; set; }

        [Required]
        [RegularExpression(@"^https?:\/\/\w+(\.\w+)*(:[0-9]+)?(\/.*)?$", ErrorMessage = "IconUrl is in wrong format!")]
        public string IconUrl { get; set; }

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
                selectedVersion.SupportedProducts = new List<string> { selectedVersion.SelectedProductId };
                versions[versions.IndexOf(editedVersion)] = selectedVersion;
            }
            else if (selectedVersion.VersionId != null)
            {
                selectedVersion.SupportedProducts = new List<string> { selectedVersion.SelectedProductId };
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

