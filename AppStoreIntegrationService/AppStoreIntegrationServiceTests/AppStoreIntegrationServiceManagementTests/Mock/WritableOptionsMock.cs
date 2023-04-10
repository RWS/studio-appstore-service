using AppStoreIntegrationServiceManagement.Repository.Interface;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceTests.AppStoreIntegrationServiceManagementTests.Mock
{
    public class WritableOptionsMock<TOptions> : IWritableOptions<TOptions> where TOptions : class, new()
    {
        private string _settings = "{}";

        public TOptions Value => JsonConvert.DeserializeObject<TOptions>(_settings);

        public void SaveOption(TOptions options) => _settings = JsonConvert.SerializeObject(options);
    }
}
