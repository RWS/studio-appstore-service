﻿using AppStoreIntegrationServiceCore.Model;
using System.Xml.Serialization;

namespace AppStoreIntegrationServiceManagement.Model.Plugins
{
    public class PluginPackage
    {
        [XmlElement(ElementName = "PlugInName")]
        public string PluginName { get; set; }
        public string Version { get; set; }
        public RequiredProduct RequiredProduct { get; set; }
        public string Author { get; set; }

        public object CreateMatchLog(ExtendedPluginDetails plugin, ExtendedPluginVersion version, List<ProductDetails> products, out bool isFullMatch)
        {
            var isNameMatch = PluginName == plugin.Name;
            var isVersionMatch = Version == version.VersionNumber;
            var isMinVersionMatch = RequiredProduct.MinimumStudioVersion == version.MinimumRequiredVersionOfStudio;
            var isMaxVersionMatch = RequiredProduct.MaximumStudioVersion == version.MaximumRequiredVersionOfStudio;
            var isAuthorMatch = Author == plugin.Developer.DeveloperName;
            var isProductMatch = IsProductMatch(version, products);
            isFullMatch = new[] { isNameMatch, isVersionMatch, isMinVersionMatch, isMaxVersionMatch, isAuthorMatch, isProductMatch }.All(match => match);
            return new { isNameMatch, isVersionMatch, isMinVersionMatch, isMaxVersionMatch, isAuthorMatch, isFullMatch, isProductMatch };
        }

        private static bool IsProductMatch(ExtendedPluginVersion version, List<ProductDetails> products)
        {
            var selectedProducts = version.SupportedProducts.SelectMany(sp => products.Where(p => p.Id == sp));
            return new[] {
                float.TryParse(selectedProducts.MinBy(p => p.MinimumStudioVersion).MinimumStudioVersion, out float minProductVersion),
                float.TryParse(selectedProducts.MaxBy(p => p.MinimumStudioVersion).MinimumStudioVersion, out float maxProductVersion),
                float.TryParse(version.MinimumRequiredVersionOfStudio, out float minRequiredVersion),
                float.TryParse(version.MinimumRequiredVersionOfStudio, out float maxRequiredVersion)
            }.All(match => match) && minProductVersion >= minRequiredVersion && maxProductVersion <= maxRequiredVersion;
        }
    }
}
