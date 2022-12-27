using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Model.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class LocalRepository : IResponseManager, IPluginManager, IProductsManager, IVersionManager, INamesManager, ICategoriesManager, ISettingsManager, ICommentsManager
    {
        private readonly IConfigurationSettings _configurationSettings;
        private readonly IWritableOptions<SiteSettings> _options;

        public LocalRepository(IConfigurationSettings configurationSettings, IWritableOptions<SiteSettings> options = null)
        {
            _configurationSettings = configurationSettings;
            _options = options;
        }

        public async Task<PluginResponse<PluginDetails<PluginVersion<string>, string>>> GetResponse()
        {
            if (string.IsNullOrEmpty(_configurationSettings.LocalPluginsFilePath))
            {
                return new PluginResponse<PluginDetails<PluginVersion<string>, string>>();
            }

            var content = await File.ReadAllTextAsync(_configurationSettings.LocalPluginsFilePath);
            return JsonConvert.DeserializeObject<PluginResponse<PluginDetails<PluginVersion<string>, string>>>(content) ?? new PluginResponse<PluginDetails<PluginVersion<string>, string>>();
        }

        public async Task<List<NameMapping>> ReadNames()
        {
            if (_configurationSettings.NameMappingsFilePath == null)
            {
                return new List<NameMapping>();
            }

            var content = await File.ReadAllTextAsync(_configurationSettings.NameMappingsFilePath);
            return JsonConvert.DeserializeObject<List<NameMapping>>(content) ?? new List<NameMapping>();
        }

        public async Task<List<ParentProduct>> ReadParents()
        {
            return (await GetResponse())?.ParentProducts;
        }

        public async Task<List<PluginDetails<PluginVersion<string>, string>>> GetPlugins()
        {
            return (await GetResponse())?.Value;
        }

        public async Task<List<ProductDetails>> ReadProducts()
        {
            return (await GetResponse())?.Products;
        }

        public async Task<string> GetVersion()
        {
            return (await GetResponse())?.APIVersion ?? "1.0.0";
        }

        public async Task<List<CategoryDetails>> ReadCategories()
        {
            return (await GetResponse())?.Categories;
        }

        public async Task SaveNames(List<NameMapping> names)
        {
            await File.WriteAllTextAsync(_configurationSettings.NameMappingsFilePath, JsonConvert.SerializeObject(names));
        }

        public async Task SaveProducts(List<ParentProduct> products)
        {
            var response = await GetResponse();
            response.ParentProducts = products;
            await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePath, JsonConvert.SerializeObject(response));
        }

        public async Task SavePlugins(List<PluginDetails<PluginVersion<string>, string>> plugins)
        {
            var response = await GetResponse();
            response.Value = plugins;
            await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePath, JsonConvert.SerializeObject(response));
        }

        public async Task SaveProducts(List<ProductDetails> products)
        {
            var response = await GetResponse();
            response.Products = products;
            await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePath, JsonConvert.SerializeObject(response));
        }

        public async Task SaveCategories(List<CategoryDetails> categories)
        {
            var response = await GetResponse();
            response.Categories = categories;
            await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePath, JsonConvert.SerializeObject(response));
        }

        public async Task SaveVersion(string version)
        {
            var response = await GetResponse();
            response.APIVersion = version;
            await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePath, JsonConvert.SerializeObject(response));
        }

        public async Task BackupPlugins(List<PluginDetails<PluginVersion<string>, string>> plugins)
        {
            var response = await GetResponse();
            response.Value = plugins;
            await File.WriteAllTextAsync(_configurationSettings.PluginsFileBackUpPath, JsonConvert.SerializeObject(response));
        }

        public async Task<SiteSettings> ReadSettings()
        {
            return new SiteSettings { Name = _options.Value.Name };
        }

        public async Task SaveSettings(SiteSettings settings)
        {
            _options.SaveOption(settings);
            _options.Value.Name = settings.Name;
        }

        public async Task<IDictionary<string, IEnumerable<Comment>>> ReadComments()
        {
            if (_configurationSettings.CommentsFilePath == null)
            {
                return new Dictionary<string, IEnumerable<Comment>>();
            }

            var content = await File.ReadAllTextAsync(_configurationSettings.CommentsFilePath);
            return JsonConvert.DeserializeObject<IDictionary<string, IEnumerable<Comment>>>(content) ?? new Dictionary<string, IEnumerable<Comment>>();
        }

        public async Task UpdateComments(IDictionary<string, IEnumerable<Comment>> comments)
        {
            await File.WriteAllTextAsync(_configurationSettings.CommentsFilePath, JsonConvert.SerializeObject(comments));
        }
    }
}
