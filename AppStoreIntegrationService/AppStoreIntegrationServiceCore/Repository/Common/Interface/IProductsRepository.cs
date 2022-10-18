using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceCore.Repository.Common.Interface
{
    public interface IProductsRepository
    {
        Task<IEnumerable<ProductDetails>> GetAllProducts();

        Task<List<ProductDetails>> ReadLocalProducts(string productsFilePath);

        Task UpdateProducts(List<ProductDetails> products);

        Task DeleteProduct(string id);
    }
}
