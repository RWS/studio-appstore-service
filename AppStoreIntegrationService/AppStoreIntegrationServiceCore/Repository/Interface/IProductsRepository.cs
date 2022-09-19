using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IProductsRepository
    {
        Task<IEnumerable<SupportedProductDetails>> GetAllProducts();

        Task<List<SupportedProductDetails>> ReadLocalProducts(string nameMappingsFilePath);

        Task UpdateProducts(List<SupportedProductDetails> namesMapping);

        Task DeleteProduct(string id);
    }
}
