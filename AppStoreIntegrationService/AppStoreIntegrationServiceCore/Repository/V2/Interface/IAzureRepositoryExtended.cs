using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.V1.Interface;

namespace AppStoreIntegrationServiceCore.Repository.V2.Interface
{
    public interface IAzureRepositoryExtended<T> : IAzureRepository<T>
    {
        Task<List<NameMapping>> GetNameMappingsFromContainer();
        Task<List<ParentProduct>> GetParentProductsFromContainer();
        Task<List<ProductDetails>> GetProductsFromContainer();
        Task<SiteSettings> GetSettingsFromContainer();
        Task UpdateNameMappingsFileBlob(string fileContent);
        Task UpdateSettingsFileBlob(string fileContent);
        Task UpdatePluginsFileBlob(string fileContent);
        Task BackupFile(string fileContent);
        Task UpdateProductsFileBlob(string fileContent);

    }
}
