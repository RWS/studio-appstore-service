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
            await UpdateCategories(categories.Where(item => item.Id != id).ToList());
        }

        public async Task<List<CategoryDetails>> GetAllCategories()
        {
            return await _categoriesManager.ReadCategories() ?? _defaultCategories;
        }

        public async Task UpdateCategories(List<CategoryDetails> categories)
        {
            await _categoriesManager.SaveCategories(categories);
        }
    }
}
