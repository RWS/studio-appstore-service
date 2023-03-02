using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface ICategoriesRepository : ICategoriesRepositoryReadonly
    {
        Task<bool> TryUpdateCategory(CategoryDetails category);
        Task DeleteCategory(string id);
        Task<CategoryDetails> GetCategoryById(string id);
    }
}
