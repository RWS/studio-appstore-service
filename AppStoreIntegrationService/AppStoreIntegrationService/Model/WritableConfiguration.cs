using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.IO;

namespace AppStoreIntegrationService.Model
{
    public class WritableConfiguration : IWritableConfiguration
    {
        private readonly IConfiguration _configuration;

        public WritableConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string this[string key] { get => _configuration[key]; set => _configuration[key] = value; }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return _configuration.GetChildren();
        }

        public IChangeToken GetReloadToken()
        {
            return _configuration.GetReloadToken();
        }

        public IConfigurationSection GetSection(string key)
        {
            return _configuration.GetSection(key);
        }

        public void SetSection(string key, string value)
        {
            var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            var foundSection = _configuration.GetSection(key);
            var newContent = File.ReadAllText(appSettingsPath).Replace($"\"{foundSection.Key}\": \"{foundSection.Value}\"", $"\"{foundSection.Key}\": \"{value}\"");
            File.WriteAllText(appSettingsPath, newContent);
        }
    }
}
