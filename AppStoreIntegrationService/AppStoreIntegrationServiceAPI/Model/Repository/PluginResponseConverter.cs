using AppStoreIntegrationServiceAPI.Model.Repository.Interface;
using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceAPI.Model.Repository
{
    public class PluginResponseConverter : IPluginResponseConverter
    {
        public List<PluginDetailsBase<PluginVersionBase<ProductDetails>, CategoryDetails>> CreateOldResponse(PluginResponseBase<PluginDetails> newResponse)
        {
            var plugins = new List<PluginDetailsBase<PluginVersionBase<ProductDetails>, CategoryDetails>>();
            foreach (var plugin in newResponse?.Value)
            {
                plugins.Add(ConvertToOldPlugin(plugin, newResponse.Categories, newResponse.Products));
            }

            return plugins;
        }

        public PluginResponseBase<PluginDetailsBase<PluginVersionBase<string>, string>> CreateBaseResponse(PluginResponseBase<PluginDetails> response)
        {
            return new PluginResponseBase<PluginDetailsBase<PluginVersionBase<string>, string>>
            {
                APIVersion = response.APIVersion,
                Value = response.Value.Select(p => PluginDetailsBase<PluginVersionBase<string>, string>.CopyFrom(p)),
                Products = response.Products,
                ParentProducts = response.ParentProducts,
                Categories = response.Categories
            };
        }

        private static PluginDetailsBase<PluginVersionBase<ProductDetails>, CategoryDetails> ConvertToOldPlugin(PluginDetails plugin, IEnumerable<CategoryDetails> categories, IEnumerable<ProductDetails> products)
        {
            var newVersions = new List<PluginVersionBase<ProductDetails>>();
            foreach (var version in plugin.Versions)
            {
                ConvertToOldVersion(version, newVersions, products);
            }

            var newPlugin = new PluginDetailsBase<PluginVersionBase<ProductDetails>, CategoryDetails>
            {
                Versions = newVersions,
                Categories = plugin.Categories?.SelectMany(category => categories?.Where(c => c.Id == category) ?? new List<CategoryDetails>()).ToList()
            };

            var properties = typeof(PluginDetailsBase<PluginVersionBase<ProductDetails>, CategoryDetails>).GetProperties().Where(p => !new[] { "Categories", "Versions" }.Any(x => x.Equals(p.Name)));
            foreach (var property in properties)
            {
                property.SetValue(newPlugin, plugin.GetType().GetProperty(property.Name).GetValue(plugin));
            }

            return newPlugin;
        }

        private static void ConvertToOldVersion(PluginVersion version, List<PluginVersionBase<ProductDetails>> newVersions, IEnumerable<ProductDetails> products)
        {
            var newVersion = new PluginVersionBase<ProductDetails>
            {
                SupportedProducts = version.SupportedProducts.SelectMany(sp => products?.Where(p => p.Id == sp) ?? new List<ProductDetails>())?.ToList()
            };

            var properties = typeof(PluginVersionBase<ProductDetails>).GetProperties().Where(p => !p.Name.Equals("SupportedProducts"));
            foreach (var property in properties)
            {
                property.SetValue(newVersion, version.GetType().GetProperty(property.Name).GetValue(version));
            }

            newVersions.Add(newVersion);
        }
    }
}