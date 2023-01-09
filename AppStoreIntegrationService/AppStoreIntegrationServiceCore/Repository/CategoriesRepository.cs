using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class CategoriesRepository : ICategoriesRepository
    {
        private readonly ICategoriesManager _categoriesManager;

        public CategoriesRepository(ICategoriesManager categoriesManager)
        {
            _categoriesManager = categoriesManager;
        }

        public async Task DeleteCategory(string id)
        {
            var categories = await _categoriesManager.ReadCategories();
            categories.ToList().Remove(categories.FirstOrDefault(c => c.Id == id));
            await _categoriesManager.SaveCategories(categories);
        }

        public async Task<IEnumerable<CategoryDetails>> GetAllCategories()
        {
            return await _categoriesManager.ReadCategories();
        }

        public async Task<CategoryDetails> GetCategoryById(string id)
        {
            var categories = await _categoriesManager.ReadCategories();
            return categories.FirstOrDefault(c => c.Id == id);
        }

        public async Task<bool> TryUpdateCategory(CategoryDetails category)
        {
            var categories = (await _categoriesManager.ReadCategories()).ToList();
            if (categories.Any(c => c.Name == category.Name && c.Id != category.Id))
            {
                return false;
            }

            var index = categories.IndexOf(categories.FirstOrDefault(c => c.Id == category.Id));
            if (index >= 0)
            {
                categories[index] = category;
            }
            else
            {
                categories.Add(category);
            }

            await _categoriesManager.SaveCategories(categories);
            return true;
        }
    }
}
