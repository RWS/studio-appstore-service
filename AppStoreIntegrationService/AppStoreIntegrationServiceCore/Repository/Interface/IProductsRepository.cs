﻿using AppStoreIntegrationServiceCore.Model;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Repository.Interface
{
    public interface IProductsRepository
    {
        Task<List<ProductDetails>> GetAllProducts();
        Task<List<ParentProduct>> GetAllParents();
        Task UpdateProducts(List<ProductDetails> products);
        Task UpdateProducts(List<ParentProduct> products);
        Task DeleteProduct(string id, ProductType type);
    }
}
