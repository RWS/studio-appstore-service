﻿using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class LocalRepository : ILocalRepository
    {
        private readonly IConfigurationSettings _configurationSettings;

        public LocalRepository(IConfigurationSettings configurationSettings)
        {
            _configurationSettings = configurationSettings;
        }

        public async Task<PluginResponse<PluginDetails<PluginVersion<string>, string>>> ReadFromFile()
        {
            if (string.IsNullOrEmpty(_configurationSettings.LocalPluginsFilePath))
            {
                return new PluginResponse<PluginDetails<PluginVersion<string>, string>>();
            }

            var content = await File.ReadAllTextAsync(_configurationSettings.LocalPluginsFilePath);
            return JsonConvert.DeserializeObject<PluginResponse<PluginDetails<PluginVersion<string>, string>>>(content);
        }

        public async Task<List<NameMapping>> ReadMappingsFromFile()
        {
            if (_configurationSettings.NameMappingsFilePath == null)
            {
                return new List<NameMapping>();
            }

            var content = await File.ReadAllTextAsync(_configurationSettings.NameMappingsFilePath);
            return JsonConvert.DeserializeObject<List<NameMapping>>(content) ?? new List<NameMapping>();
        }

        public async Task<List<ParentProduct>> ReadParentsFromFile()
        {
            return (await ReadFromFile())?.ParentProducts;
        }

        public async Task<List<PluginDetails<PluginVersion<string>, string>>> ReadPluginsFromFile()
        {
            return (await ReadFromFile())?.Value;
        }

        public async Task<List<ProductDetails>> ReadProductsFromFile()
        {
            return (await ReadFromFile())?.Products;
        }

        public async Task SaveMappingsToFile(List<NameMapping> names)
        {
            await File.WriteAllTextAsync(_configurationSettings.NameMappingsFilePath, JsonConvert.SerializeObject(names));
        }

        public async Task SaveParentsToFile(List<ParentProduct> products)
        {
            var response = await ReadFromFile();
            var text = JsonConvert.SerializeObject(new PluginResponse<PluginDetails<PluginVersion<string>, string>>
            {
                APIVersion = response.APIVersion,
                Value = response.Value,
                Products = response.Products,
                ParentProducts = products,
                Categories = response.Categories
            });
            await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePath, text);
        }

        public async Task SavePluginsToFile(List<PluginDetails<PluginVersion<string>, string>> plugins)
        {
            var response = await ReadFromFile();
            var text = JsonConvert.SerializeObject(new PluginResponse<PluginDetails<PluginVersion<string>, string>>
            {
                APIVersion = response.APIVersion,
                Value = plugins,
                Products = response.Products,
                ParentProducts = response.ParentProducts,
                Categories = response.Categories
            });
            await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePath, text);
        }

        public async Task SaveProductsToFile(List<ProductDetails> products)
        {
            var response = await ReadFromFile();
            var text = JsonConvert.SerializeObject(new PluginResponse<PluginDetails<PluginVersion<string>, string>>
            {
                APIVersion = response.APIVersion,
                Value = response.Value,
                Products = products,
                ParentProducts = response.ParentProducts,
                Categories = response.Categories
            });
            await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePath, text);
        }

        public async Task<string> GetAPIVersionFromFile()
        {
            return (await ReadFromFile())?.APIVersion ?? "1.0.0";
        }

        public async Task SaveCategoriesToFile(List<CategoryDetails> categories)
        {
            var response = await ReadFromFile();
            var text = JsonConvert.SerializeObject(new PluginResponse<PluginDetails<PluginVersion<string>, string>>
            {
                APIVersion = response.APIVersion,
                Value = response.Value,
                Products = response.Products,
                ParentProducts = response.ParentProducts,
                Categories = categories
            });
            await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePath, text);
        }

        public async Task<List<CategoryDetails>> ReadCategoriesFromFile()
        {
            return (await ReadFromFile())?.Categories;
        }

        public async Task SaveAPIVersionToFile(string version)
        {
            var response = await ReadFromFile();
            var text = JsonConvert.SerializeObject(new PluginResponse<PluginDetails<PluginVersion<string>, string>>
            {
                APIVersion = version,
                Value = response.Value,
                Products = response.Products,
                ParentProducts = response.ParentProducts,
                Categories = response.Categories
            });
            await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePath, text);
        }
    }
}
