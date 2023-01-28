using AppStoreIntegrationServiceCore.Model.Common.Interface;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AppStoreIntegrationServiceCore.Model
{
    public class WritableOptions<TOptions> : IWritableOptions<TOptions>
        where TOptions : class, new()
    {
        private readonly IOptions<TOptions> _options;

        public WritableOptions(IOptions<TOptions> options)
        {
            _options = options;
        }

        public TOptions Value { get => _options.Value; }

        public void SaveOption(TOptions options)
        {
            var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            var json = JObject.Parse(File.ReadAllText(appSettingsPath));
            var stringToJToken = JToken.Parse(JsonConvert.SerializeObject(options));
            json[typeof(TOptions).Name] = stringToJToken;
            File.WriteAllText(appSettingsPath, json.ToString());
        }
    }
}
