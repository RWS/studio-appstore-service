﻿using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.V1.Interface;

namespace AppStoreIntegrationServiceCore.Repository.V2.Interface
{
    public interface IAzureRepositoryExtended<T> : IAzureRepository<T>
    {
        Task<List<NameMapping>> GetNameMappingsFromContainer();
        Task<List<ParentProduct>> GetParentProductsFromContainer();
        Task<List<ProductDetails>> GetProductsFromContainer();
        Task<List<CategoryDetails>> GetCategoriesFromContainer();
        Task<SiteSettings> GetSettingsFromContainer();
        Task<string> GetAPIVersionFromContainer();
        Task UpdateMappingsFileBlob(List<NameMapping> mappings);
        Task UpdateSettingsFileBlob(SiteSettings settings);
        Task UpdatePluginsFileBlob(List<T> plugins);
        Task UpdateProductsFileBlob(List<ProductDetails> products);
        Task UpdateParentsFileBlob(List<ParentProduct> products);
        Task UpdateCategoriesFileBlob(List<CategoryDetails> products);
        Task BackupFile(List<T> plugins);
    }
}