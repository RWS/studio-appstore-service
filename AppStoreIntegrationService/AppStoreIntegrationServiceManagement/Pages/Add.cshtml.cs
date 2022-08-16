using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppStoreIntegrationServiceManagement.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class AddModel : PageModel
    {
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

        [BindProperty]
        public SupportedProductDetails SelectedProduct { get; set; }

        [BindProperty]
        public string SelectedVersionId { get; set; }

        public SelectList CategoryListItems { get; set; }

        private readonly IPluginRepository _pluginsRepository;
        private readonly IHttpContextAccessor _contextAccessor;

        public AddModel(IPluginRepository pluginsRepository, IHttpContextAccessor contextAccessor)
        {
            _pluginsRepository = pluginsRepository;
            _contextAccessor = contextAccessor;
        }

        public async Task<IActionResult> OnPostAddPluginAsync()
        {
            PrivatePlugin.IconUrl = GetDefaultIcon();
            await SetAvailableCategories();

            SelectedVersionDetails = new PluginVersion
            {
                VersionName = PrivatePlugin.NewVersionNumber,
                VersionNumber = PrivatePlugin.NewVersionNumber,
                IsPrivatePlugin = true,
                AppHasStudioPluginInstaller = true,
                Id = Guid.NewGuid().ToString(),
            };
            SelectedVersionId = SelectedVersionDetails.Id;
            SelectedVersionDetails.SetSupportedProducts();

            PrivatePlugin.Versions = new List<PluginVersion>
            {
                SelectedVersionDetails
            };
            Versions.Add(SelectedVersionDetails);

            SetSelectedProducts(Versions, string.Empty);

            return Page();
        }

        public async Task<IActionResult> OnPostGoToPage(string pageUrl)
        {
            var modalDetails = new ModalMessage
            {
                RequestPage = $"{pageUrl}",
                ModalType = ModalType.WarningMessage,
                Title = "Unsaved changes!",
                Message = $"Discard changes for {PrivatePlugin.Name}?"
            };

            return Partial("_ModalPartial", modalDetails);
        }

        public async Task<IActionResult> OnPostSavePlugin()
        {
            var modalDetails = new ModalMessage
            {
                RequestPage = "add",
            };
            if (IsValid())
            {
                await SetValues();
                try
                {
                    await _pluginsRepository.AddPrivatePlugin(PrivatePlugin);
                    modalDetails.ModalType = ModalType.SuccessMessage;
                    modalDetails.Message = $"{PrivatePlugin.Name} was added.";
                }
                catch (Exception e)
                {
                    modalDetails.ModalType = ModalType.WarningMessage;
                    modalDetails.Message = e.Message;
                }
            }
            else
            {
                modalDetails.Message = "Please fill all required values.";
                modalDetails.ModalType = ModalType.WarningMessage;
            }

            return Partial("_ModalPartial", modalDetails);
        }

        public async Task<IActionResult> OnPostAddVersion()
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

        public async Task<IActionResult> OnPostSaveVersionForPluginAsync()
        {
            var modalDetails = new ModalMessage
            {
                RequestPage = "add",
            };
            if (IsValid())
            {
                await SetValues();
                try
                {
                    await _pluginsRepository.AddPrivatePlugin(PrivatePlugin);
                    modalDetails.ModalType = ModalType.SuccessMessage;
                    modalDetails.Message = $"{PrivatePlugin.Name} was added.";
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

        public async Task<IActionResult> OnPostShowVersionDetails()
        {
            var version = Versions.FirstOrDefault(v => v.Id.Equals(SelectedVersionId));
            version.IsNewVersion = true;
            ModelState.Clear();
            return Partial("_PluginVersionDetailsPartial", version);
        }

        private async Task SetAvailableCategories()
        {
            Categories = await _pluginsRepository.GetCategories();
            CategoryListItems = new SelectList(Categories, nameof(CategoryDetails.Id), nameof(CategoryDetails.Name));

        }

        private bool IsValid()
        {
            var generalDetailsContainsNull = AnyNull(PrivatePlugin.Name, PrivatePlugin.Description, PrivatePlugin.IconUrl);

            if (!string.IsNullOrEmpty(SelectedVersionId) && SelectedVersionDetails != null)
            {
                var detailsContainsNull = AnyNull(SelectedVersionDetails.VersionNumber, SelectedVersionDetails.MinimumRequiredVersionOfStudio, SelectedVersionDetails.DownloadUrl);
                if (generalDetailsContainsNull || detailsContainsNull)
                {
                    return false;
                }
            }
            return !generalDetailsContainsNull;
        }
        private async Task SetValues()
        {
            PrivatePlugin.Name = PrivatePlugin.Name.Trim();
            SetVersionList();
            await SetCategoryList();
            // This method will be removed later after studio release. We had to move the download url  from plugin to version details. Studio still uses the url from the plugin details
            SetDownloadUrl();
        }

        private void SetDownloadUrl()
        {
            PrivatePlugin.DownloadUrl = PrivatePlugin.Versions.LastOrDefault()?.DownloadUrl;
        }

        private void SetVersionList()
        {
            var editedVersion = Versions.FirstOrDefault(v => v.Id.Equals(SelectedVersionDetails.Id));
            var selectedProduct = SelectedVersionDetails.SupportedProducts.FirstOrDefault(item => item.Id == SelectedVersionDetails.SelectedProductId);
            if (editedVersion != null)
            {
                SelectedVersionDetails.SupportedProducts = new List<SupportedProductDetails> { selectedProduct };
                Versions[Versions.IndexOf(editedVersion)] = SelectedVersionDetails;
            }
            else if (SelectedVersionDetails?.SelectedProduct != null)
            {
                SelectedVersionDetails.SupportedProducts = new List<SupportedProductDetails> { selectedProduct };
                Versions.Add(SelectedVersionDetails);
            }

            PrivatePlugin.Versions = Versions;
        }

        private async Task SetCategoryList()
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

        private static void SetSelectedProducts(List<PluginVersion> versions, string versionName)
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
                        if (!version.IsNewVersion)
                        {
                            version.VersionName = $"{version.SelectedProduct.ProductName} - {version.VersionNumber}";
                        }
                        else
                        {
                            version.VersionName = versionName;
                        }
                    }
                }
            }
        }

        public static bool AnyNull(params object[] objects)
        {
            return objects.Any(s => s == null);
        }

        public string GetDefaultIcon()
        {
            var scheme = _contextAccessor.HttpContext?.Request?.Scheme;
            var host = _contextAccessor.HttpContext?.Request?.Host.Value;
            return $"{scheme}://{host}/images/plugin.ico";
        }
    }
}