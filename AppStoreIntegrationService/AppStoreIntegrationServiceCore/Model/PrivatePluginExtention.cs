namespace AppStoreIntegrationServiceCore.Model
{
    public static class PrivatePluginExtention
    {
        public static PluginDetails<PluginVersion<string>, string> ConvertToPluginDetails
        (
            this ExtendedPluginDetails privateDetails,
            PluginDetails<PluginVersion<string>,  string> foundDetails,
            ExtendedPluginVersion selectedVersionDetails
        )
        {
            return new PluginDetails<PluginVersion<string>, string>
            {
                Versions = PrepareVersions(foundDetails.Versions, selectedVersionDetails)
            };
        }

        private static List<PluginVersion<string>> PrepareVersions(List<PluginVersion<string>> versions, ExtendedPluginVersion selectedVersion)
        {
            if (selectedVersion.VersionNumber == null)
            {
                return versions;
            }

            var newVersionList = new List<PluginVersion<string>>(versions);
            var existingVersion = newVersionList.FirstOrDefault(v => v.VersionId.Equals(selectedVersion.VersionId));
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
