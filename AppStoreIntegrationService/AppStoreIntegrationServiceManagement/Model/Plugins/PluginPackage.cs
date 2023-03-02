using AppStoreIntegrationServiceCore.Model;
using AppStoreIntegrationServiceManagement.Helpers;
using System.IO.Compression;
using System.Text;
using System.Xml;
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

        public (bool, bool) CreatePluginMatchLog(PluginDetails plugin, out bool isFullMatch)
        {
            var isNameMatch = PluginName == plugin.Name;
            var isAuthorMatch = Author == plugin.Developer.DeveloperName;
            isFullMatch = new[] { isNameMatch, isAuthorMatch }.All(match => match);
            return (isNameMatch, isAuthorMatch);
        }

        public VersionManifestComparison CreateVersionMatchLog(PluginVersion version, IEnumerable<ProductDetails> products, out bool isFullMatch)
        {
            var isVersionMatch = Version == version.VersionNumber;
            var isMinVersionMatch = RequiredProduct.MinimumStudioVersion == version.MinimumRequiredVersionOfStudio;
            var isMaxVersionMatch = RequiredProduct.MaximumStudioVersion == version.MaximumRequiredVersionOfStudio;
            var isProductMatch = IsProductMatch(version, products);
            isFullMatch = new[] { isVersionMatch, isMinVersionMatch, isMaxVersionMatch, isProductMatch }.All(match => match);
            return new VersionManifestComparison
            {
                IsVersionMatch = isVersionMatch,
                IsMinVersionMatch = isMinVersionMatch,
                IsMaxVersionMatch = isMaxVersionMatch,
                IsProductMatch = isProductMatch
            };
        }

        private static bool IsProductMatch(PluginVersion version, IEnumerable<ProductDetails> products)
        {
            var selectedProducts = version.SupportedProducts.SelectMany(sp => products.Where(p => p.Id == sp));
            return new[] {
                System.Version.TryParse(selectedProducts.MinBy(p => p.MinimumStudioVersion).MinimumStudioVersion, out Version minProductVersion),
                System.Version.TryParse(selectedProducts.MaxBy(p => p.MinimumStudioVersion).MinimumStudioVersion, out Version maxProductVersion),
                System.Version.TryParse(version.MinimumRequiredVersionOfStudio, out Version minRequiredVersion),
                System.Version.TryParse(version.MaximumRequiredVersionOfStudio, out Version maxRequiredVersion)
            }.All(match => match) && minProductVersion >= minRequiredVersion && maxProductVersion <= maxRequiredVersion;
        }

        public static async Task<PluginPackage> DownloadPlugin(string downloadUrl)
        {
            var _pluginDownloadPath = $"{Environment.CurrentDirectory}/Temp";
            if (!Directory.Exists(_pluginDownloadPath))
            {
                Directory.CreateDirectory(_pluginDownloadPath);
            }

            try
            {
                var reader = new RemoteStreamReader(new Uri(downloadUrl));
                var stream = await reader.ReadAsStreamAsync();
                using (var fileStream = File.Create($"{_pluginDownloadPath}/Plugin.sdlplugin"))
                {
                    stream.CopyTo(fileStream);
                }

                ZipFile.ExtractToDirectory($"{_pluginDownloadPath}/Plugin.sdlplugin", _pluginDownloadPath);
                var response = ImportFromFile($"{_pluginDownloadPath}/pluginpackage.manifest.xml");
                Directory.Delete(_pluginDownloadPath, true);
                return response;
            }
            catch (InvalidDataException)
            {
                Directory.Delete(_pluginDownloadPath, true);
                throw new Exception("Unable to execute manifest comparison! There is no manifest in the URL!");
            }
            catch (Exception e)
            {
                Directory.Delete(_pluginDownloadPath, true);
                throw new Exception(e.Message);
            }
        }

        private static PluginPackage ImportFromFile(string file)
        {
            var serializer = new XmlSerializer(typeof(PluginPackage));
            var result = new StringBuilder();
            using (var reader = new StreamReader(file))
            {
                while (reader.Peek() >= 0)
                {
                    result.AppendLine(reader.ReadLine());
                }
            }

            if (result.Length > 0)
            {
                try
                {
                    return (PluginPackage)serializer.Deserialize(new XmlReaderNamespaceIgnore(new StringReader(result.ToString())));
                }
                catch (XmlException)
                {
                    return null;
                }

            }

            return null;
        }
    }
}
