using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Model
{
    public class ExtendedPluginDetails : PluginDetails<ExtendedPluginVersion, string>
    {
        public ExtendedPluginDetails() : base() { }

        public ExtendedPluginDetails(PluginDetails<PluginVersion<string>, string> other)
        {
            PluginDetails<ExtendedPluginVersion, string> test = new ExtendedPluginDetails();

            var thisProperties = typeof(PluginDetails<ExtendedPluginVersion, string>).GetProperties();
            var otherProperties = typeof(PluginDetails<PluginVersion<string>, string>).GetProperties();

            for (var i = 0; i < thisProperties.Length; i++)
            {
                if (thisProperties[i].Name != "Versions")
                {
                    thisProperties[i].SetValue(this, otherProperties[i].GetValue(other));
                }
            }

            Versions = other.Versions.Select(v => new ExtendedPluginVersion(v)).ToList();
        }

        [JsonIgnore]
        public bool IsEditMode { get; set; }
        [JsonIgnore]
        public MultiSelectList CategoryListItems { get; set; }
        [JsonIgnore]
        public string SelectedVersionId { get; set; }

        public bool IsValid(PluginVersion<string> selectedVersion)
        {
            return selectedVersion.VersionId != null || IsEditMode;
        }

        public void SetVersionList(List<ExtendedPluginVersion> versions, ExtendedPluginVersion selectedVersion)
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

        public void SetDownalodUrl()
        {
            DownloadUrl = Versions.LastOrDefault()?.DownloadUrl;
        }
    }
}

