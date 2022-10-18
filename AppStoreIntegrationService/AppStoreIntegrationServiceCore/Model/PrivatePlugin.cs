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

        public string ChangelogLink { get; set; }

        public string SupportUrl { get; set; }

        public string SupportEmail { get; set; }

        public bool Inactive { get; set; }

        [Required]
        [MinLength(5)]
        public string DownloadUrl { get; set; }

        public List<CategoryDetails> Categories { get; set; }

        public List<ExtendedPluginVersion<string>> Versions { get; set; }
        public string NewVersionNumber { get; set; }

        [Required]
        public string IconUrl { get; set; }

        public string DeveloperName { get; set; }

        public bool IsEditMode { get; set; }

        public bool IsValid(PluginVersion<string> selectedVersion)
        {
            return selectedVersion.Id != null || IsEditMode;
        }

        public void SetVersionList(List<ExtendedPluginVersion<string>> versions, ExtendedPluginVersion<string> selectedVersion, List<ProductDetails> products)
        {
            var editedVersion = versions.FirstOrDefault(v => v.Id.Equals(selectedVersion.Id));
            var selectedProduct = products?.FirstOrDefault(item => item.Id == selectedVersion.SelectedProductId);
            if (editedVersion != null)
            {
                selectedVersion.SupportedProducts = new List<string> { selectedVersion.SelectedProductId };
                versions[versions.IndexOf(editedVersion)] = selectedVersion;
            }
            else if (selectedVersion.Id != null)
            {
                selectedVersion.SupportedProducts = new List<string> { selectedVersion.SelectedProductId };
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
    }
}

