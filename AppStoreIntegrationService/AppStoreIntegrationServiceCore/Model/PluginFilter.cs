using System.Text.RegularExpressions;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginFilter
    {
        public string Query { get; set; }
        public string StudioVersion { get; set; }
        public string SortOrder { get; set; }
        public string Price { get; set; }
        public List<int> CategoryId { get; set; }
        public Status Status { get; set; }
        public string SupportedProduct { get; set; }

        private static IEnumerable<PluginDetails> FilterByStatus(IEnumerable<PluginDetails> plugins, Status status)
        {
            if (status.Equals(Status.Active))
            {
                plugins = plugins.Where(x => x.Status.Equals(Status.Active));
                foreach (var plugin in plugins)
                {
                    plugin.Versions = plugin.Versions.Where(v => v.VersionStatus.Equals(Status.Active)).ToList();
                }
            }

            return status switch
            {
                Status.Inactive => plugins.Where(x => x.Status.Equals(Status.Inactive)),
                Status.Draft => plugins.Where(x => x.Status.Equals(Status.Draft) || x.Versions.Any(v => v.VersionStatus.Equals(Status.Draft))),
                Status.InReview => plugins.Where(x => x.Status.Equals(Status.InReview) || x.Versions.Any(v => v.VersionStatus.Equals(Status.InReview))),
                _ => plugins
            };
        }

        private static IEnumerable<PluginDetails> FilterByQuery(IEnumerable<PluginDetails> pluginsList, string query)
        {
            return pluginsList.Where(p => Regex.IsMatch(p.Name, query, RegexOptions.IgnoreCase));
        }

        private static IEnumerable<PluginDetails> FilterByPrice(IEnumerable<PluginDetails> pluginsList, string price)
        {
            return pluginsList.Where(p => p.PaidFor == (!string.IsNullOrEmpty(price) && price.Equals("paid", StringComparison.CurrentCultureIgnoreCase)));
        }

        private static IEnumerable<PluginDetails> FilterByVersion(IEnumerable<PluginDetails> pluginsList, string studioVersion, IEnumerable<ProductDetails> products)
        {
            var plugins = new List<PluginDetails>();
            var expression = new Regex("\\d+", RegexOptions.IgnoreCase);
            var versionNumber = expression.Match(studioVersion);
            var oldTradosName = $"SDL Trados Studio {versionNumber.Value}";
            var rebrandedStudioName = $"Trados Studio {versionNumber.Value}";

            foreach (var plugin in pluginsList)
            {
                var matchingVersions = new List<PluginVersion>();

                foreach (var version in plugin.Versions)
                {
                    var product = version.SupportedProducts.SelectMany(sp => products
                                                           .Where(p => p.Id == sp))
                                                           .FirstOrDefault(s => s.ProductName.Equals(oldTradosName) ||
                                                                                s.ProductName.Equals(rebrandedStudioName) ||
                                                                                s.ProductName.Equals(rebrandedStudioName + "(SR2)"));
                    if (product != null)
                    {
                        matchingVersions.Add(version);
                        plugin.DownloadUrl = version.DownloadUrl;
                    }
                }

                if (matchingVersions.Any())
                {
                    plugin.Versions = matchingVersions;
                    plugins.Add(plugin);
                }
            }

            return plugins;
        }

        private static IEnumerable<PluginDetails> FilterByCategory(IEnumerable<PluginDetails> pluginsList, IEnumerable<int> categoryIds)
        {
            return categoryIds.SelectMany(c => pluginsList.Where(p => p.Categories.Any(pc => pc.Equals(c))));
        }

        public static IEnumerable<PluginDetails> SearchPlugins(IEnumerable<PluginDetails> pluginsList, PluginFilter filter, IEnumerable<ProductDetails> products)
        {
            pluginsList ??= new List<PluginDetails>();

            var searchedPluginList = FilterByStatus(pluginsList, filter.Status);
            if (!string.IsNullOrEmpty(filter?.SupportedProduct))
            {
                searchedPluginList = FilterByProduct(searchedPluginList, filter.SupportedProduct);
            }

            if (!string.IsNullOrEmpty(filter?.Query))
            {
                searchedPluginList = FilterByQuery(searchedPluginList, filter.Query);
            }

            if (!string.IsNullOrEmpty(filter?.Price))
            {
                searchedPluginList = FilterByPrice(searchedPluginList, filter.Price);
            }

            if (!string.IsNullOrEmpty(filter?.StudioVersion))
            {
                searchedPluginList = FilterByVersion(searchedPluginList, filter.StudioVersion, products);
            }

            if (filter?.CategoryId?.Count > 0)
            {
                searchedPluginList = FilterByCategory(searchedPluginList, filter.CategoryId);
            }

            return searchedPluginList;
        }

        private static IEnumerable<PluginDetails> FilterByProduct(IEnumerable<PluginDetails> plugins, string product)
        {
            return plugins.Where(p => p.Versions.Select(v => v.SupportedProducts.Any(p => p.Equals(product))).Any(check => check));
        }
    }
}
