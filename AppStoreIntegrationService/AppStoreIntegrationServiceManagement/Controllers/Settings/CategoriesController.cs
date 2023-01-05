using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Controllers.Settings
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
            return View(await _categoriesRepository.GetAllCategories());
        }

        [HttpPost]
        public async Task<IActionResult> Add()
        {
            var categories = await _categoriesRepository.GetAllCategories();
            return PartialView("_NewCategoryPartial", new CategoryDetails
            {
                Id = SetIndex(categories),
            });
        }

        [HttpPost]
        public async Task<IActionResult> Update(CategoryDetails category)
        {
            if (await _categoriesRepository.TryUpdateCategory(category))
            {
                TempData["StatusMessage"] = "Success! Categories were updated!";
                return Content("/Settings/Categories");
            };

            return PartialView("_StatusMessage", "Error! There is already a category with this name!");
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

        private async Task<bool> IsInUse(string id)
        {
            var plugins = await _pluginRepository.GetAll(null);
            return plugins.Select(p => p.Categories.Any(c => c == id)).Any(item => item);
        }

        private static string SetIndex(IEnumerable<CategoryDetails> categories)
        {
            var lastCategory = categories.MaxBy(c => c.Id);
            if (lastCategory == null)
            {
                return "1";
            }

            return (int.Parse(lastCategory.Id) + 1).ToString();
        }
    }
}
