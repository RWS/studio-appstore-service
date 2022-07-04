using Microsoft.Extensions.Configuration;

namespace AppStoreIntegrationService.Model
{
    public interface IWritableConfiguration : IConfiguration
    {
        void SetSection(string key, string value);
    }
}
