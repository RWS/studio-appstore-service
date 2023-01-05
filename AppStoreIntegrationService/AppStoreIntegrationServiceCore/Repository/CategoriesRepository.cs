using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class CategoriesRepository : ICategoriesRepository
    {
        private readonly ICategoriesManager _categoriesManager;
        private readonly List<CategoryDetails> _defaultCategories;

        public CategoriesRepository(ICategoriesManager categoriesManager)
        {
            _categoriesManager = categoriesManager;
            _defaultCategories = new List<CategoryDetails>
            {
                new CategoryDetails
                {
                    Name = "File filters & converters",
                    Id = "2"
                },
                new CategoryDetails
                {
                    Name = "Translation memory",
                    Id = "3"
                },
                new CategoryDetails
                {
                    Name = "Terminology",
                    Id = "4"
                },
                new CategoryDetails
                {
                    Name = "Process automation & management",
                    Id = "5"
                },
                new CategoryDetails
                {
                    Name = "Automated translation",
                    Id = "6"
                },
                new CategoryDetails
                {
                    Name = "Reference",
                    Id = "7"
                }
            };
        }

        public async Task DeleteCategory(string id)
        {
            var categories = await _categoriesManager.ReadCategories() ?? _defaultCategories;
            categories.ToList().Remove(categories.FirstOrDefault(c => c.Id == id));
            await _categoriesManager.SaveCategories(categories);
        }

        public async Task<IEnumerable<CategoryDetails>> GetAllCategories()
        {
            return await _categoriesManager.ReadCategories() ?? _defaultCategories;
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
