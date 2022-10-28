using AppStoreIntegrationServiceCore.Model;

namespace ResponseConverter.ViewModel
{
    public class PluginResponseConverter<T, U>
        where T : PluginDetails<PluginVersion<string>, string>, new()
        where U : PluginDetails<PluginVersion<ProductDetails>, CategoryDetails>, new()
    {
        private readonly PluginResponse<T> _newResponse;
        private readonly List<U> _plugins;

        public PluginResponseConverter(PluginResponse<T> oldResponse)
        {
            _newResponse = oldResponse;
            _plugins = new ();
        }

        public PluginResponse<U> CreateOldResponse()
        {
            foreach (var plugin in _newResponse.Value)
            {
                ConvertToOldPlugin(plugin);
            }

            return new PluginResponse<U>
            {
                Value = _plugins,
            };
        }

        private void ConvertToOldPlugin(T plugin)
        {
            var newVersions = new List<PluginVersion<ProductDetails>>();
            foreach (var version in plugin.Versions)
            {
                ConvertToOldVersion(version, newVersions);
            }

            var newPlugin = new U
            {
                Versions = newVersions,
                Categories = plugin.Categories.SelectMany(category => _newResponse.Categories.Where(c => c.Id == category)).ToList()
            };

            var properties = typeof(PluginDetails<PluginVersion<ProductDetails>, CategoryDetails>).GetProperties().Where(p => !new[] { "Categories", "Versions" }.Any(x => x.Equals(p.Name)));
            foreach (var property in properties)
            {
                property.SetValue(newPlugin, plugin.GetType().GetProperty(property.Name).GetValue(plugin));
            }

            _plugins.Add(newPlugin);
        }

        private void ConvertToOldVersion(PluginVersion<string> version, List<PluginVersion<ProductDetails>> newVersions)
        {
            var oldVersion = new PluginVersion<ProductDetails>
            {
                SupportedProducts = new List<ProductDetails> { _newResponse.Products.FirstOrDefault(p => p.Id == version.SupportedProducts[0]) }
            };

            var properties = typeof(PluginVersion<ProductDetails>).GetProperties().Where(p => !p.Name.Equals("SupportedProducts"));
            foreach (var property in properties)
            {
                property.SetValue(oldVersion, version.GetType().GetProperty(property.Name).GetValue(version));
            }

            newVersions.Add(oldVersion);
        }
    }
}
