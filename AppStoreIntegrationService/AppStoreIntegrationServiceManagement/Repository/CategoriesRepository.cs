using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository;
using AppStoreIntegrationServiceCore.Repository.Interface;
using AppStoreIntegrationServiceManagement.Repository.Interface;

namespace AppStoreIntegrationServiceManagement.Repository
{
    public class CategoriesRepository : CategoriesRepositoryBase, ICategoriesRepository
    {
        private readonly IResponseManager _responseManager;

        public CategoriesRepository(IResponseManager responseManager) : base(responseManager)
        {
            _responseManager = responseManager;
        }

        public async Task DeleteCategory(string id)
        {
            var categories = await GetAllCategories();
            await SaveCategories(categories.Where(c => c.Id != id));
        }

        public async Task SaveCategories(IEnumerable<CategoryDetails> categories)
        {
            var data = await _responseManager.GetResponse();
            data.Categories = categories;
            await _responseManager.SaveResponse(data);
        }

        public async Task<CategoryDetails> GetCategoryById(string id)
        {
            var categories = await GetAllCategories();
            return categories.FirstOrDefault(c => c.Id == id);
        }

        public async Task<bool> TryUpdateCategory(CategoryDetails category)
        {
            var categories = (await GetAllCategories()).ToList();
            if (category == null || categories.Any(c => c.IsDuplicate(category)))
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

            await SaveCategories(categories);
            return true;
        }
    }
}
