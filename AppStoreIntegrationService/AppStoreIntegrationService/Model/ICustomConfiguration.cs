using Microsoft.Extensions.Configuration;

namespace AppStoreIntegrationService.Model
{
    public interface ICustomConfiguration : IConfiguration
    {
        void SetSection(string key, string value);
    }
}
