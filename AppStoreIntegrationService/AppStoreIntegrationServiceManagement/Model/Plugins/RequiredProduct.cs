using System.Xml.Serialization;

namespace AppStoreIntegrationServiceManagement.Model.Plugins
{
    public class RequiredProduct
    {
        [XmlAttribute(AttributeName = "minversion")]
        public string MinimumStudioVersion { get; set; }
        [XmlAttribute(AttributeName = "maxversion")]
        public string MaximumStudioVersion { get; set; }
    }
}
