using AppStoreIntegrationServiceAPI.Model.Repository.Interface;
using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceAPI.Model.Repository
{
    public class PluginResponseConverter<T, U> : IPluginResponseConverter<T, U>
        where T : PluginDetails<PluginVersion<string>, string>, new()
        where U : PluginDetails<PluginVersion<ProductDetails>, CategoryDetails>, new()
    {
        public PluginResponse<U> CreateOldResponse(PluginResponse<T> newResponse)
        {
            var plugins = new List<U>();
            foreach (var plugin in newResponse?.Value)
            {
                plugins.Add(ConvertToOldPlugin(plugin, newResponse.Categories, newResponse.Products));
            }

            return new PluginResponse<U>
            {
                Value = plugins,
            };
        }

        private static U ConvertToOldPlugin(T plugin, List<CategoryDetails> categories, List<ProductDetails> products)
        {
            var newVersions = new List<PluginVersion<ProductDetails>>();
            foreach (var version in plugin.Versions)
            {
                ConvertToOldVersion(version, newVersions, products);
            }

            var newPlugin = new U
            {
                Versions = newVersions,
                Categories = plugin.Categories.SelectMany(category => categories.Where(c => c.Id == category)).ToList()
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
                SupportedProducts = new List<ProductDetails> { products.FirstOrDefault(p => p.Id == version.SupportedProducts[0]) }
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
