using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceManagement.Pages
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IPluginRepository _pluginRepository;
        private readonly IHttpContextAccessor _contextAccessor;

        [BindProperty]
        public PrivatePlugin PrivatePlugin { get; set; }
        [BindProperty]
        public List<int> SelectedCategories { get; set; }

        [BindProperty]
        public List<PluginVersion> Versions { get; set; }

        [BindProperty]
        public List<CategoryDetails> Categories { get; set; }

        [BindProperty]
        public PluginVersion SelectedVersionDetails { get; set; }

        public SelectList CategoryListItems { get; set; }

        [BindProperty]
        public string SelectedVersionId { get; set; }

        public EditModel(IPluginRepository pluginRepository, IHttpContextAccessor contextAccessor)
        {
            _pluginRepository = pluginRepository;
            _contextAccessor = contextAccessor;
            Categories = new List<CategoryDetails>();
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            await SetSelectedPlugin(id);
            Categories = await _pluginRepository.GetCategories();
            SetSelectedCategories();

            return Page();
        }

        public async Task<IActionResult> OnPostGoToPage(string pageUrl)
        {
            var foundPluginDetails = await _pluginRepository.GetPluginById(PrivatePlugin.Id);
            if (IsSaved(foundPluginDetails))
            {
                return Redirect(pageUrl);
            }

            var modalDetails = new ModalMessage
            {
                ModalType = ModalType.WarningMessage,
                Title = "Warning!",
                Message = $"There is unsaved data for {PrivatePlugin.Name}. Discard changes?",
                RequestPage = $"{pageUrl}"
            };

            return Partial("_ModalPartial", modalDetails);
        }

        private bool IsSaved(PluginDetails foundPluginDetails)
        {
            SetCategoryList();
            var newPluginDetails = PrivatePlugin.ConvertToPluginDetails(foundPluginDetails, SelectedVersionDetails);
            return JsonConvert.SerializeObject(newPluginDetails) == JsonConvert.SerializeObject(foundPluginDetails);
        }

        public async Task<IActionResult> OnPostSavePluginAsync()
        {
            var modalDetails = new ModalMessage();
            if (IsValid())
            {
                SetEditedValues();
                try
                {
                    await _pluginRepository.UpdatePrivatePlugin(PrivatePlugin);
                    modalDetails.ModalType = ModalType.SuccessMessage;
                    modalDetails.Message = $"{PrivatePlugin.Name} was updated.";
                    modalDetails.Id = PrivatePlugin.Id;
                }
                catch (Exception e)
                {
                    modalDetails.ModalType = ModalType.WarningMessage;
                    modalDetails.Message = e.Message;
                    modalDetails.Id = PrivatePlugin.Id;
                }
            }
            else
            {
                modalDetails.Title = string.Empty;
                modalDetails.Message = "Please fill all required values.";
                modalDetails.ModalType = ModalType.WarningMessage;
            }

            return Partial("_ModalPartial", modalDetails);
        }
        public IActionResult OnPostAddVersion()
        {
            SelectedVersionDetails = new PluginVersion
            {
                VersionNumber = string.Empty,
                IsPrivatePlugin = true,
                IsNewVersion = true,
                Id = Guid.NewGuid().ToString()
            };
            SelectedVersionDetails.SetSupportedProducts();

            Versions.Add(SelectedVersionDetails);
            SetSelectedProducts(Versions, "New plugin version");
            ModelState.Clear();

            return Partial("_PluginVersionDetailsPartial", SelectedVersionDetails);
        }

        public async Task<IActionResult> OnPostDeleteVersionAsync(string id)
        {
            await _pluginRepository.RemovePluginVersion(PrivatePlugin.Id, id);
            return Page();
        }

        public async Task<IActionResult> OnPostShowVersionDetails()
        {
            var version = Versions.FirstOrDefault(v => v.Id.Equals(SelectedVersionId));
            version.IsNewVersion = false;
            ModelState.Clear();
            return Partial("_PluginVersionDetailsPartial", version);
        }

        public static bool AnyNull(params object[] objects)
        {
            return objects.Any(s => s == null);
        }

        private bool IsValid()
        {
            var generalDetailsContainsNull = AnyNull(PrivatePlugin.Name, PrivatePlugin.Description, PrivatePlugin.IconUrl);

            if (SelectedVersionDetails.Id != null)
            {
                var detailsContainsNull = AnyNull(SelectedVersionDetails.VersionNumber, SelectedVersionDetails.MinimumRequiredVersionOfStudio, SelectedVersionDetails.DownloadUrl);
                if (generalDetailsContainsNull || detailsContainsNull)
                {
                    return false;
                }
            }
            return !generalDetailsContainsNull;
        }

        private void SetEditedValues()
        {
            PrivatePlugin.Name = PrivatePlugin.Name.Trim();
            SetVersionList();
            SetCategoryList();
            // This method will be removed later after studio release. We had to move the download url  from plugin to version details. Studio still uses the url from the plugin details
            SetDownloadUrl();
        }

        private void SetDownloadUrl()
        {
            PrivatePlugin.DownloadUrl = PrivatePlugin.Versions.LastOrDefault()?.DownloadUrl;
        }

        private void SetCategoryList()
        {
            PrivatePlugin.Categories = new List<CategoryDetails>();
            foreach (var categoryId in SelectedCategories)
            {
                var category = Categories.FirstOrDefault(c => c.Id.Equals(categoryId));
                if (category != null)
                {
                    PrivatePlugin.Categories.Add(category);
                }
            }
        }

        private void SetVersionList()
        {
            var editedVersion = Versions.FirstOrDefault(v => v.Id.Equals(SelectedVersionDetails.Id));

            if (editedVersion != null)
            {
                Versions[Versions.IndexOf(editedVersion)] = SelectedVersionDetails;
            }
            else if (SelectedVersionDetails?.SelectedProduct != null)
            {
                var selectedProduct = SelectedVersionDetails.SupportedProducts.FirstOrDefault(item => item.Id == SelectedVersionDetails.SelectedProductId);
                SelectedVersionDetails.SupportedProducts = new List<SupportedProductDetails> { selectedProduct };
                Versions.Add(SelectedVersionDetails);
            }

            PrivatePlugin.Versions = Versions;
        }

        private void SetSelectedCategories()
        {
            CategoryListItems = new SelectList(Categories, nameof(CategoryDetails.Id), nameof(CategoryDetails.Name));
            var selectedCategories = PrivatePlugin.Categories.Select(c => c.Id).ToList();

            SelectedCategories = selectedCategories;
        }

        private void SetSelectedProducts(List<PluginVersion> versions, string versionName)
        {
            foreach (var version in versions)
            {
                if (version.SelectedProduct is null)
                {
                    var lastSupportedProduct = version.SupportedProducts?.LastOrDefault();
                    if (lastSupportedProduct != null)
                    {
                        version.SelectedProductId = lastSupportedProduct.Id;
                        version.SelectedProduct = lastSupportedProduct;
                        version.VersionName = version.IsNewVersion ? versionName : $"{version.SelectedProduct.ProductName} - {version.VersionNumber}";
                    }
                }
            }
        }

        private async Task SetSelectedPlugin(int id)
        {
            var pluginDetails = await _pluginRepository.GetPluginById(id);

            Versions = pluginDetails.Versions;
            PrivatePlugin = new PrivatePlugin
            {
                Id = pluginDetails.Id,
                PaidFor = pluginDetails.PaidFor,
                DeveloperName = pluginDetails.Developer?.DeveloperName,
                Description = pluginDetails.Description,
                Name = pluginDetails.Name,
                Categories = pluginDetails.Categories,
                Versions = pluginDetails.Versions,
                IconUrl = pluginDetails.Icon.MediaUrl
            };
            if (string.IsNullOrEmpty(PrivatePlugin.IconUrl))
            {
                PrivatePlugin.IconUrl = GetDefaultIcon();
            }

            SetSelectedProducts(PrivatePlugin.Versions, string.Empty);
        }

        public string GetDefaultIcon()
        {
            var scheme = _contextAccessor.HttpContext?.Request?.Scheme;
            var host = _contextAccessor.HttpContext?.Request?.Host.Value;
            return $"{scheme}://{host}/images/plugin.ico";
        }
    }
}