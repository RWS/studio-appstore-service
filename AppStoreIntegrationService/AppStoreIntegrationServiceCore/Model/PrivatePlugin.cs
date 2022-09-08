using System.ComponentModel.DataAnnotations;

namespace AppStoreIntegrationServiceCore.Model
{
    public class PrivatePlugin
    {
        public int Id { get; set; }

        [Required]
        [MinLength(5)]
        public string Name { get; set; }

        [Required]
        [MinLength(20)]
        public string Description { get; set; }

        public bool PaidFor { get; set; }

        public bool Inactive { get; set; }

        [Required]
        [MinLength(5)]
        public string DownloadUrl { get; set; }

        public List<CategoryDetails> Categories { get; set; }

        public List<PluginVersion> Versions { get; set; }
        public string NewVersionNumber { get; set; }

        [Required]
        public string IconUrl { get; set; }

        public string DeveloperName { get; set; }

        public bool IsEditMode { get; set; }

        public bool IsValid(PluginVersion selectedVersion)
        {
            var generalDetailsContainsNull = AnyNull(Name, Description, IconUrl);
            if (!(selectedVersion.Id == null && IsEditMode))
            {
                var detailsContainsNull = AnyNull(selectedVersion.VersionNumber, selectedVersion.MinimumRequiredVersionOfStudio, selectedVersion.DownloadUrl);
                if (generalDetailsContainsNull || detailsContainsNull)
                {
                    return false;
                }
            }
            return !generalDetailsContainsNull;
        }

        public void SetVersionList(List<PluginVersion> versions, PluginVersion selectedVersion)
        {
            var editedVersion = versions.FirstOrDefault(v => v.Id.Equals(selectedVersion.Id));
            var selectedProduct = selectedVersion.SupportedProducts.FirstOrDefault(item => item.Id == selectedVersion.SelectedProductId);
            if (editedVersion != null)
            {
                if (!IsEditMode)
                {
                    selectedVersion.SupportedProducts = new List<SupportedProductDetails> { selectedProduct };
                }

                versions[versions.IndexOf(editedVersion)] = selectedVersion;
            }
            else if (selectedVersion.Id != null)
            {
                selectedVersion.SupportedProducts = new List<SupportedProductDetails> { selectedProduct };
                versions.Add(selectedVersion);
            }

            Versions = versions;
        }

        public void SetCategoryList(List<int> selectedCategories, List<CategoryDetails> categories)
        {
            Categories = selectedCategories?.SelectMany(categoriId => categories.Where(category => category.Id == categoriId)).ToList();
        }

        public void SetDownloadUrl()
        {
            DownloadUrl = Versions.LastOrDefault()?.DownloadUrl;
        }

        private static bool AnyNull(params object[] objects)
        {
            return objects.Any(s => s == null);
        }
    }
}

