using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface ICategoriesRepository
    {
        Task<List<CategoryDetails>> GetAllCategories();
        Task<bool> TryUpdateCategory(CategoryDetails category);
        Task DeleteCategory(string id);
        Task<CategoryDetails> GetCategoryById(string id);
    }
}
