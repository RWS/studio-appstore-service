namespace AppStoreIntegrationServiceCore.Model
{
    public static class PrivatePluginExtention
    {
        public static PluginDetails<PluginVersion<string>, string> ConvertToPluginDetails
        (
            this PrivatePlugin<PluginVersion<string>> privateDetails,
            PluginDetails<PluginVersion<string>,  string> foundDetails,
            ExtendedPluginVersion<string> selectedVersionDetails
        )
        {
            return new PluginDetails<PluginVersion<string>, string>
            {
                Id = privateDetails.Id,
                Name = privateDetails.Name,
                Icon = new IconDetails { MediaUrl = privateDetails.IconUrl },
                Developer = string.IsNullOrEmpty(privateDetails.DeveloperName) ? null : new DeveloperDetails { DeveloperName = privateDetails.DeveloperName },
                ChangelogLink = privateDetails.ChangelogLink,
                SupportEmail = privateDetails.SupportEmail,
                SupportUrl = privateDetails.SupportUrl,
                Description = privateDetails.Description,
                PaidFor = privateDetails.PaidFor,
                Inactive = privateDetails.Inactive,
                Categories = privateDetails.Categories,
                DownloadUrl = foundDetails.DownloadUrl,
                Versions = PrepareVersions(foundDetails.Versions, selectedVersionDetails)
            };
        }

        private static List<PluginVersion<string>> PrepareVersions(List<PluginVersion<string>> versions, ExtendedPluginVersion<string> selectedVersion)
        {
            if (selectedVersion.VersionName == null)
            {
                return versions;
            }

            var newVersionList = new List<PluginVersion<string>>(versions);
            var existingVersion = newVersionList.FirstOrDefault(v => v.VersionId.Equals(selectedVersion.VersionId));
            selectedVersion.SupportedProducts = new List<string> { selectedVersion.SelectedProductId };
            if (Equals(existingVersion, null))
            {
                newVersionList.Add(selectedVersion);
            }
            else
            {
                newVersionList[newVersionList.IndexOf(existingVersion)] = selectedVersion;
            }
            
            return newVersionList;
        }
    }
}
