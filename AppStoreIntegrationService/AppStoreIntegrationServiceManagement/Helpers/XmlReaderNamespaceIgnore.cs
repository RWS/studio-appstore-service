using System.Xml;

namespace AppStoreIntegrationServiceManagement.Helpers
{
    public class XmlReaderNamespaceIgnore : XmlTextReader
    {
        public XmlReaderNamespaceIgnore(TextReader reader) : base(reader) { }

        public override string NamespaceURI { get => ""; }
    }
}
