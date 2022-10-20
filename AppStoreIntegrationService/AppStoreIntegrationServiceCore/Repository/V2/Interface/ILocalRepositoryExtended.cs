using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.V1.Interface;

namespace AppStoreIntegrationServiceCore.Repository.V2.Interface
{
    public interface ILocalRepositoryExtended<T> : ILocalRepository<T>
    {
        Task SavePluginsToFile(List<T> plugins);
        Task SaveProductsToFile(List<ProductDetails> products);
        Task SaveParentsToFile(List<ParentProduct> products);
        Task SaveMappingsToFile(List<NameMapping> names);
        Task SaveCategoriesToFile(List<CategoryDetails> categories);
        Task<List<ProductDetails>> ReadProductsFromFile();
        Task<List<ParentProduct>> ReadParentsFromFile();
        Task<List<NameMapping>> ReadMappingsFromFile();
        Task<List<CategoryDetails>> ReadCategoriesFromFile();
        Task<string> GetAPIVersionFromFile();
    }
}
