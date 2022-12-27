using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface ICategoriesManager
    {
        Task SaveCategories(List<CategoryDetails> categories);
        Task<List<CategoryDetails>> ReadCategories();
    }
}
