using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Model;
using AppStoreIntegrationServiceManagement.Model.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceManagement.Controllers.Categories
{
    [Area("Settings")]
    [Authorize(Policy = "IsAdmin")]
    public class CategoriesController : Controller
    {
        public readonly ICategoriesRepository _categoriesRepository;
        private readonly IPluginRepository _pluginRepository;

        public CategoriesController(ICategoriesRepository categoriesRepository, IPluginRepository pluginRepository)
        {
            _categoriesRepository = categoriesRepository;
            _pluginRepository = pluginRepository;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoriesRepository.GetAllCategories();
            return View(new CategoriesModel
            {
                Categories = categories
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddNew()
        {
            var categories = await _categoriesRepository.GetAllCategories();
            return PartialView("_NewCategoryPartial", new CategoryDetails
            {
                Id = SetIndex(categories),
            });
        }

        [HttpPost]
        public async Task<IActionResult> Add(CategoryDetails category, List<CategoryDetails> categories)
        {
            if (TryValidateProduct(category, categories, out IActionResult result))
            {
                categories.Add(category);
                await _categoriesRepository.UpdateCategories(categories);
            }

            return result;
        }

        [HttpPost]
        public async Task<IActionResult> Update(List<CategoryDetails> categories)
        {
            if (!categories.Any(item => item.IsValid()))
            {
                return PartialView("_StatusMessage", "Error! Parameter cannot be null!");
            }

            if (ExistDuplicate(categories))
            {
                return PartialView("_StatusMessage", "Error! There is already a category with this name!");
            }

            await _categoriesRepository.UpdateCategories(categories);
            TempData["StatusMessage"] = "Success! Categories were updated!";
            return Content("/Settings/Categories");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (await IsInUse(id))
            {
                TempData["StatusMessage"] = "Error! This category is used by plugins!";
                return Content("/Settings/Categories");
            }

            await _categoriesRepository.DeleteCategory(id);
            TempData["StatusMessage"] = "Success! Category was deleted!";
            return Content("/Settings/Categories");
        }

        [Route("[controller]/[action]/{redirectUrl?}")]
        [HttpPost]
        public async Task<IActionResult> GoToPage(string redirectUrl, CategoryDetails product, List<CategoryDetails> products)
        {
            redirectUrl = redirectUrl.Replace('.', '/');

            if (string.IsNullOrEmpty(product.Name) &&
                await AreSavedCategories(products))
            {
                return Content(redirectUrl);
            }

            var modalDetails = new ModalMessage
            {
                ModalType = ModalType.WarningMessage,
                Title = "Warning!",
                Message = $"Discard changes for category?",
                RequestPage = $"{redirectUrl}"
            };

            return PartialView("_ModalPartial", modalDetails);
        }

        private async Task<bool> AreSavedCategories(List<CategoryDetails> categories)
        {
            var savedCategories = await _categoriesRepository.GetAllCategories();
            return JsonConvert.SerializeObject(savedCategories) == JsonConvert.SerializeObject(categories);
        }

        private async Task<bool> IsInUse(string id)
        {
            var plugins = await _pluginRepository.GetAll(null);
            return plugins.Select(p => p.Categories.Any(c => c == id)).Any(item => item);
        }

        private static bool ExistDuplicate(List<CategoryDetails> categories)
        {
            return !categories.GroupBy(p => p.Name, (_, products) => products
                              .Count() == 1)
                              .All(item => item);
        }

        private static string SetIndex(List<CategoryDetails> categories)
        {
            var lastCategory = categories.MaxBy(c => c.Id);
            if (lastCategory == null)
            {
                return "1";
            }

            return (int.Parse(lastCategory.Id) + 1).ToString();
        }

        private bool TryValidateProduct(CategoryDetails category, List<CategoryDetails> categories, out IActionResult result)
        {
            if (string.IsNullOrEmpty(category.Name))
            {
                result = PartialView("_StatusMessage", "Error! Parameter cannot be null!");
                return false;
            }

            if (categories.Any(p => p.Equals(category)))
            {
                result = PartialView("_StatusMessage", "Error! There are duplicated params!");
                return false;
            }

            TempData["StatusMessage"] = "Success! Category was added!";
            result = Content("/Settings/Categories");
            return true;
        }
    }
}
