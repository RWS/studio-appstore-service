using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceCore.Repository.Interface;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceCoreTests.Mock
{
    public class AzureRepositoryMock : IResponseManager, IPluginManager, IProductsManager, IVersionManager, INamesManager, ICategoriesManager, ISettingsManager, ICommentsManager, ILogsManager
    {
        private readonly string _data;

        public AzureRepositoryMock(string data) 
        {
            _data = data;
        }

        public async Task<PluginResponse<PluginDetails>> GetResponse()
        {
            if (string.IsNullOrEmpty(_data))
            {
                return new PluginResponse<PluginDetails>();
            }

            return JsonConvert.DeserializeObject<PluginResponse<PluginDetails>>(_data) ?? new PluginResponse<PluginDetails>();
        }

        public async Task<string> GetVersion()
        {
            var response = await GetResponse();
            return response.APIVersion;
        }

        public Task<IEnumerable<CategoryDetails>> ReadCategories()
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<int, CommentPackage>> ReadComments()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PluginDetails>> ReadDrafts()
        {
            throw new NotImplementedException();
        }

        public async Task<IDictionary<int, IEnumerable<Log>>> ReadLogs()
        {
            var response = await GetResponse();
            return response.Logs;
        }

        public Task<IEnumerable<NameMapping>> ReadNames()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ParentProduct>> ReadParents()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PluginDetails>> ReadPending()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PluginDetails>> ReadPlugins()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProductDetails>> ReadProducts()
        {
            throw new NotImplementedException();
        }

        public Task<SiteSettings> ReadSettings()
        {
            throw new NotImplementedException();
        }

        public Task SaveCategories(IEnumerable<CategoryDetails> categories)
        {
            throw new NotImplementedException();
        }

        public Task SaveDrafts(IEnumerable<PluginDetails> plugins)
        {
            throw new NotImplementedException();
        }

        public Task SaveNames(IEnumerable<NameMapping> names)
        {
            throw new NotImplementedException();
        }

        public Task SavePending(IEnumerable<PluginDetails> plugins)
        {
            throw new NotImplementedException();
        }

        public Task SavePlugins(IEnumerable<PluginDetails> plugins)
        {
            throw new NotImplementedException();
        }

        public Task SaveProducts(IEnumerable<ProductDetails> products)
        {
            throw new NotImplementedException();
        }

        public Task SaveProducts(IEnumerable<ParentProduct> products)
        {
            throw new NotImplementedException();
        }

        public Task SaveSettings(SiteSettings settings)
        {
            throw new NotImplementedException();
        }

        public Task SaveVersion(string version)
        {
            throw new NotImplementedException();
        }

        public Task UpdateComments(IDictionary<int, CommentPackage> package)
        {
            throw new NotImplementedException();
        }

        public Task UpdateLogs(IDictionary<int, IEnumerable<Log>> package)
        {
            throw new NotImplementedException();
        }
    }
}
