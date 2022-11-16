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
    }
}
