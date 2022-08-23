namespace AppStoreIntegrationServiceCore.Model
{
    public static class PrivatePluginExtention
    {
        public static PluginDetails ConvertToPluginDetails(this PrivatePlugin privateDetails, PluginDetails foundDetails, PluginVersion selectedVersionDetails)
        {
            return new PluginDetails
            {
                Id = privateDetails.Id,
                Name = privateDetails.Name,
                Icon = new IconDetails { MediaUrl = privateDetails.IconUrl },
                Developer = string.IsNullOrEmpty(privateDetails.DeveloperName) ? null : new DeveloperDetails { DeveloperName = privateDetails.DeveloperName },
                Description = privateDetails.Description,
                PaidFor = privateDetails.PaidFor,
                Inactive = privateDetails.Inactive,
                Categories = privateDetails.Categories,
                DownloadUrl = foundDetails.DownloadUrl,
                Versions = PrepareVersions(foundDetails.Versions, selectedVersionDetails)
            };
        }

        public static bool IsValid(this PrivatePlugin plugin, PluginVersion version, bool isEditMode)
        {
            var generalDetailsContainsNull = AnyNull(plugin.Name, plugin.Description, plugin.IconUrl);
            if (!(version.Id == null && isEditMode))
            {
                var detailsContainsNull = AnyNull(version.VersionNumber, version.MinimumRequiredVersionOfStudio, version.DownloadUrl);
                if (generalDetailsContainsNull || detailsContainsNull)
                {
                    return false;
                }
            }
            return !generalDetailsContainsNull;
        }

        public static void SetVersionList(this PrivatePlugin plugin, List<PluginVersion> versions, PluginVersion version, bool isEditMode)
        {
            version.SetSupportedProducts();
            var editedVersion = versions.FirstOrDefault(v => v.Id.Equals(version.Id));
            var selectedProduct = version.SupportedProducts.FirstOrDefault(item => item.Id == version.SelectedProductId);
            if (editedVersion != null)
            {
                if (!isEditMode)
                {
                    version.SupportedProducts = new List<SupportedProductDetails> { selectedProduct };
                }
                
                versions[versions.IndexOf(editedVersion)] = version;
            }
            else if(version.Id != null)
            {
                version.SupportedProducts = new List<SupportedProductDetails> { selectedProduct };
                versions.Add(version);
            }

            plugin.Versions = versions;
        }

        public static void SetCategoryList(this PrivatePlugin plugin, List<int> selectedCategories, List<CategoryDetails> categories)
        {
            plugin.Categories = selectedCategories?.SelectMany(categoriId => categories.Where(category => category.Id == categoriId)).ToList();
        }

        public static void SetDownloadUrl(this PrivatePlugin plugin)
        {
            plugin.DownloadUrl = plugin.Versions.LastOrDefault()?.DownloadUrl;
        }

        private static bool AnyNull(params object[] objects)
        {
            return objects.Any(s => s == null);
        }

        private static List<PluginVersion> PrepareVersions(List<PluginVersion> versions, PluginVersion selectedVersionDetails)
        {
            if (selectedVersionDetails.VersionName == null)
            {
                return versions;
            }

            var newVersionList = new List<PluginVersion>(versions);
            var existingVersion = newVersionList.FirstOrDefault(v => v.Id.Equals(selectedVersionDetails.Id));
            if (existingVersion != null)
            {
                newVersionList[newVersionList.IndexOf(existingVersion)] = selectedVersionDetails;
                return newVersionList;
            }

            newVersionList.Add(selectedVersionDetails);
            return newVersionList;
        }
    }
}
