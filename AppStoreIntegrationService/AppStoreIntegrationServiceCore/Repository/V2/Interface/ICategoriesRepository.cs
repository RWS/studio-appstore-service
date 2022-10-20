using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.V2.Interface
{
    public interface ICategoriesRepository
    {
        Task<List<CategoryDetails>> GetAllCategories();
        Task UpdateCategories(List<CategoryDetails> categories);
        Task DeleteCategory(string id);
    }
}
