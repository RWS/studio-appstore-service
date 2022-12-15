using System.Xml;

namespace AppStoreIntegrationServiceManagement.Model.Plugins
{
    public class XmlReaderNamespaceIgnore : XmlTextReader
    {
        public XmlReaderNamespaceIgnore(TextReader reader) : base(reader) { }

        public override string NamespaceURI { get => ""; }
    }
}
