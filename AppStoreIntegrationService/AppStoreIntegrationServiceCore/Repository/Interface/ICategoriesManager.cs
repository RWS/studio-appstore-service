using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface ICategoriesManager
    {
        Task SaveCategories(IEnumerable<CategoryDetails> categories);
        Task<IEnumerable<CategoryDetails>> ReadCategories();
    }
}
