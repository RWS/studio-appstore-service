using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface ICategoriesRepositoryReadonly
    {
        Task<IEnumerable<CategoryDetails>> GetAllCategories();
    }
}