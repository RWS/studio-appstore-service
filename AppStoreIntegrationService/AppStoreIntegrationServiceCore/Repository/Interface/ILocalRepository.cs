using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface ILocalRepository<T>
    {
        Task<List<T>> ReadPluginsFromFile();
        Task SavePluginsToFile(List<T> plugins);
        Task SaveProductsToFile(List<ProductDetails> products);
        Task SaveParentsToFile(List<ParentProduct> products);
        Task SaveMappingsToFile(List<NameMapping> names);
        Task SaveCategoriesToFile(List<CategoryDetails> categories);
        Task SaveAPIVersionToFile(string version);
        Task<List<ProductDetails>> ReadProductsFromFile();
        Task<List<ParentProduct>> ReadParentsFromFile();
        Task<List<NameMapping>> ReadMappingsFromFile();
        Task<List<CategoryDetails>> ReadCategoriesFromFile();
        Task<string> GetAPIVersionFromFile();
    }
}
