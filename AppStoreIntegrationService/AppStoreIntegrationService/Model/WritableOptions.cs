using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace AppStoreIntegrationService.Model
{
    public class WritableOptions<TOptions> : IWritableOptions<TOptions>
        where TOptions : class, new()
    {
        public TOptions Value { get => GetOption(); }

        public void SaveOption(TOptions options)
        {
            var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            var json = JObject.Parse(File.ReadAllText(appSettingsPath));
            var stringToJToken = JToken.Parse(JsonConvert.SerializeObject(options));
            json[typeof(TOptions).Name] = stringToJToken;
            File.WriteAllText(appSettingsPath, json.ToString());
        }

        private TOptions GetOption()
        {
            var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            var json = JObject.Parse(File.ReadAllText(appSettingsPath));
            return JsonConvert.DeserializeObject<TOptions>(json[typeof(TOptions).Name].ToString());
        }
    }
}
