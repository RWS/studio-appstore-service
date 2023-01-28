using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Model.Common.Interface;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Repository
{
    public class LocalRepository : IResponseManager, IPluginManager, IProductsManager, IVersionManager, INamesManager, ICategoriesManager, ISettingsManager, ICommentsManager, ILogsManager
    {
        private readonly IConfigurationSettings _configurationSettings;
        private readonly IWritableOptions<SiteSettings> _options;

        public LocalRepository(IConfigurationSettings configurationSettings, IWritableOptions<SiteSettings> options = null)
        {
            _configurationSettings = configurationSettings;
            _options = options;
        }

        public async Task<PluginResponse<PluginDetails>> GetResponse()
        {
            if (string.IsNullOrEmpty(_configurationSettings.LocalPluginsFilePath))
            {
                return new PluginResponse<PluginDetails>();
            }

            var content = await File.ReadAllTextAsync(_configurationSettings.LocalPluginsFilePath);

            if (content == null)
            {
                return new PluginResponse<PluginDetails>();
            }

            return JsonConvert.DeserializeObject<PluginResponse<PluginDetails>>(content) ?? new PluginResponse<PluginDetails>();
        }

        public async Task<IEnumerable<NameMapping>> ReadNames()
        {
            return (await GetResponse())?.Names;
        }

        public async Task<IEnumerable<ParentProduct>> ReadParents()
        {
            return (await GetResponse())?.ParentProducts;
        }

        public async Task<IEnumerable<PluginDetails>> ReadPlugins()
        {
            return (await GetResponse())?.Value;
        }

        public async Task<IEnumerable<ProductDetails>> ReadProducts()
        {
            return (await GetResponse())?.Products;
        }

        public async Task<string> GetVersion()
        {
            return (await GetResponse())?.APIVersion ?? "1.0.0";
        }

        public async Task<IEnumerable<CategoryDetails>> ReadCategories()
        {
            return (await GetResponse())?.Categories;
        }

        public async Task<IEnumerable<PluginDetails>> ReadPending()
        {
            return (await GetResponse())?.Pending;
        }

        public async Task SavePending(IEnumerable<PluginDetails> plugins)
        {
            var response = await GetResponse();
            response.Pending = plugins;
            await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePath, JsonConvert.SerializeObject(response));
        }

        public async Task<IEnumerable<PluginDetails>> ReadDrafts()
        {
            return (await GetResponse())?.Drafts;
        }

        public async Task SaveDrafts(IEnumerable<PluginDetails> plugins)
        {
            var response = await GetResponse();
            response.Drafts = plugins;
            await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePath, JsonConvert.SerializeObject(response));
        }

        public async Task<SiteSettings> ReadSettings()
        {
            return new SiteSettings { Name = _options.Value.Name };
        }
        public async Task<IDictionary<int, CommentPackage>> ReadComments()
        {
            return (await GetResponse())?.Comments;
        }
        public async Task<IDictionary<int, IEnumerable<Log>>> ReadLogs()
        {
            return (await GetResponse())?.Logs;
        }

        public async Task SaveNames(IEnumerable<NameMapping> names)
        {
            var response = await GetResponse();
            response.Names = names;
            await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePath, JsonConvert.SerializeObject(response));
        }

        public async Task SaveProducts(IEnumerable<ParentProduct> products)
        {
            var response = await GetResponse();
            response.ParentProducts = products;
            await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePath, JsonConvert.SerializeObject(response));
        }

        public async Task SavePlugins(IEnumerable<PluginDetails> plugins)
        {
            var response = await GetResponse();
            response.Value = plugins;
            await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePath, JsonConvert.SerializeObject(response));
        }

        public async Task SaveProducts(IEnumerable<ProductDetails> products)
        {
            var response = await GetResponse();
            response.Products = products;
            await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePath, JsonConvert.SerializeObject(response));
        }

        public async Task SaveCategories(IEnumerable<CategoryDetails> categories)
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

        public async Task SaveSettings(SiteSettings settings)
        {
            _options.SaveOption(settings);
            _options.Value.Name = settings.Name;
        }

        public async Task UpdateComments(IDictionary<int, CommentPackage> comments)
        {
            var response = await GetResponse();
            response.Comments = comments;
            await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePath, JsonConvert.SerializeObject(response));
        }

        public async Task UpdateLogs(IDictionary<int, IEnumerable<Log>> logs)
        {
            var response = await GetResponse();
            response.Logs = logs;
            await File.WriteAllTextAsync(_configurationSettings.LocalPluginsFilePath, JsonConvert.SerializeObject(response));
        }
    }
}
