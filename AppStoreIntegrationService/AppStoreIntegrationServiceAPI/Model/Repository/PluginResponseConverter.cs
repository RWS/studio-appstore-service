using AppStoreIntegrationServiceAPI.Model.Repository.Interface;
using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceAPI.Model.Repository
{
    public class PluginResponseConverter : IPluginResponseConverter
    {
        public PluginResponse<PluginDetails<PluginVersion<ProductDetails>, CategoryDetails>> CreateOldResponse(PluginResponse<PluginDetails<PluginVersion<string>, string>> newResponse)
        {
            var plugins = new List<PluginDetails<PluginVersion<ProductDetails>, CategoryDetails>>();
            foreach (var plugin in newResponse?.Value)
            {
                plugins.Add(ConvertToOldPlugin(plugin, newResponse.Categories, newResponse.Products));
            }

            return new PluginResponse<PluginDetails<PluginVersion<ProductDetails>, CategoryDetails>>
            {
                Value = plugins,
            };
        }

        private static PluginDetails<PluginVersion<ProductDetails>, CategoryDetails> ConvertToOldPlugin(PluginDetails<PluginVersion<string>, string> plugin, List<CategoryDetails> categories, List<ProductDetails> products)
        {
            var newVersions = new List<PluginVersion<ProductDetails>>();
            foreach (var version in plugin.Versions)
            {
                ConvertToOldVersion(version, newVersions, products);
            }

            var newPlugin = new PluginDetails<PluginVersion<ProductDetails>, CategoryDetails>
            {
                Versions = newVersions,
                Categories = plugin.Categories?.SelectMany(category => categories?.Where(c => c.Id == category) ?? new List<CategoryDetails>()).ToList()
            };

            var properties = typeof(PluginDetails<PluginVersion<ProductDetails>, CategoryDetails>).GetProperties().Where(p => !new[] { "Categories", "Versions" }.Any(x => x.Equals(p.Name)));
            foreach (var property in properties)
            {
                property.SetValue(newPlugin, plugin.GetType().GetProperty(property.Name).GetValue(plugin));
            }

            return newPlugin;
        }

        private static void ConvertToOldVersion(PluginVersion<string> version, List<PluginVersion<ProductDetails>> newVersions, List<ProductDetails> products)
        {
            var oldVersion = new PluginVersion<ProductDetails>
            {
                SupportedProducts = version.SupportedProducts.SelectMany(sp => products?.Where(p => p.Id == sp) ?? new List<ProductDetails>())?.ToList()
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
