namespace AppStoreIntegrationServiceCore.Model
{
    public static class PrivatePluginExtention
    {
        public static PluginDetails ConvertToPluginDetails(this PrivatePlugin privateDetails, PluginDetails foundDetails, PluginVersion selectedVersionDetails, List<SupportedProductDetails> products)
        {
            return new PluginDetails
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
                Categories = privateDetails.Categories.Any() ? privateDetails.Categories : foundDetails.Categories,
                DownloadUrl = foundDetails.DownloadUrl,
                Versions = PrepareVersions(foundDetails.Versions, selectedVersionDetails, products)
            };
        }

        private static List<PluginVersion> PrepareVersions(List<PluginVersion> versions, PluginVersion selectedVersion, List<SupportedProductDetails> products)
        {
            if (selectedVersion.VersionName == null)
            {
                return versions;
            }

            var newVersionList = new List<PluginVersion>(versions);
            var existingVersion = newVersionList.FirstOrDefault(v => v.Id.Equals(selectedVersion.Id));
            var selectedProduct = products?.FirstOrDefault(p => p.Id == selectedVersion.SelectedProductId);
            selectedVersion.SupportedProducts = new List<SupportedProductDetails> { selectedProduct };
            newVersionList[newVersionList.IndexOf(existingVersion)] = selectedVersion;
            return newVersionList;
        }
    }
}
