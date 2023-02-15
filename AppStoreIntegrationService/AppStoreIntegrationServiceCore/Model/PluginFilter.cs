using System.Text.RegularExpressions;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginFilter
    {
        public string Query { get; set; }
        public string BaseProduct { get; set; }
        public string StudioVersion { get; set; }
        public string SortOrder { get; set; }
        public string Price { get; set; }
        public List<int> CategoryId { get; set; }
        public Status Status { get; set; }
        public string SupportedProduct { get; set; }

        private static IEnumerable<PluginDetails> FilterByStatus(IEnumerable<PluginDetails> plugins, Status status)
        {
            if (status == Status.Active)
            {
                plugins = plugins.Where(x => x.Status == Status.Active);
                foreach (var plugin in plugins)
                {
                    plugin.Versions = plugin.Versions.Where(v => v.VersionStatus == Status.Active).ToList();
                }
            }

            return status switch
            {
                Status.Inactive => plugins.Where(x => x.Status.Equals(Status.Inactive)),
                Status.Draft => plugins.Where(x => x.Status.Equals(Status.Draft) || x.Drafts.Any(v => v.VersionStatus.Equals(Status.Draft))),
                Status.InReview => plugins.Where(x => x.Status.Equals(Status.InReview) || x.Pending.Any(v => v.VersionStatus.Equals(Status.InReview))),
                _ => plugins
            };
        }

        private static IEnumerable<PluginDetails> FilterByQuery(IEnumerable<PluginDetails> pluginsList, string query)
        {
            return pluginsList.Where(p => Regex.IsMatch(p.Name, query, RegexOptions.IgnoreCase));
        }

        private static IEnumerable<PluginDetails> FilterByPrice(IEnumerable<PluginDetails> pluginsList, string price)
        {
            return pluginsList.Where(p => p.PaidFor == price.Equals("paid", StringComparison.CurrentCultureIgnoreCase));
        }

        private static IEnumerable<PluginDetails> FilterByVersion(IEnumerable<PluginDetails> pluginsList, PluginFilter filter, IEnumerable<ProductDetails> products, IEnumerable<ParentProduct> parents)
        {
            var plugins = new List<PluginDetails>();

            foreach (var plugin in pluginsList)
            {
                var matchingVersions = new List<PluginVersion>();

                foreach (var version in plugin.Versions)
                {
                    if (GetSupportedProduct(version, filter, products, parents) != null)
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

        private static ProductDetails GetSupportedProduct(PluginVersion version, PluginFilter filter, IEnumerable<ProductDetails> products, IEnumerable<ParentProduct> parents)
        {
            if (Version.TryParse(filter.StudioVersion, out _))
            {
                return version.SupportedProducts.SelectMany(x => products
                                                .Where(y => y.Id == x))
                                                .FirstOrDefault(x => filter.BaseProduct == parents
                                                .FirstOrDefault(y => y.Id == x.ParentProductID).ProductName && IsMatchingVersion(version, filter.StudioVersion));
            }

            var expression = new Regex("\\d+", RegexOptions.IgnoreCase);
            var versionNumber = expression.Match(filter.StudioVersion);
            var oldTradosName = $"SDL Trados Studio {versionNumber.Value}";
            var rebrandedStudioName = $"Trados Studio {versionNumber.Value}";

            return version.SupportedProducts.SelectMany(x => products
                                            .Where(y => y.Id == x))
                                            .FirstOrDefault(x => x.ProductName.Equals(oldTradosName) ||
                                                                 x.ProductName.Equals(rebrandedStudioName) ||
                                                                 x.ProductName.Equals(rebrandedStudioName + " (SR2)"));

        }

        private static bool IsMatchingVersion(PluginVersion version, string studioVersion)
        {
            var parsedStudioVersion = new Version(studioVersion);
            return new Version(version.MinimumRequiredVersionOfStudio ?? "0.0.0") <= parsedStudioVersion &&
                   new Version(version.MaximumRequiredVersionOfStudio ?? "0.0.0") >= parsedStudioVersion;
        }

        private static IEnumerable<PluginDetails> FilterByCategory(IEnumerable<PluginDetails> pluginsList, IEnumerable<int> categoryIds)
        {
            return categoryIds.SelectMany(c => pluginsList.Where(p => p.Categories.Any(pc => pc == c.ToString())));
        }

        public static IEnumerable<PluginDetails> FilterPlugins(IEnumerable<PluginDetails> pluginsList, PluginFilter filter, IEnumerable<ProductDetails> products, IEnumerable<ParentProduct> parents)
        {
            pluginsList ??= new List<PluginDetails>();

            if (filter == null)
            {
                return pluginsList;
            }

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
                products ??= new List<ProductDetails>();
                parents ??= new List<ParentProduct>();
                searchedPluginList = FilterByVersion(searchedPluginList, filter, products, parents);
            }

            if (filter?.CategoryId?.Count > 0)
            {
                searchedPluginList = FilterByCategory(searchedPluginList, filter.CategoryId);
            }

            return searchedPluginList;
        }

        private static IEnumerable<PluginDetails> FilterByProduct(IEnumerable<PluginDetails> plugins, string product)
        {
            return plugins.Where(p => p.Versions.Any(v => v.SupportedProducts.Any(p => p == product)));
        }
    }
}
