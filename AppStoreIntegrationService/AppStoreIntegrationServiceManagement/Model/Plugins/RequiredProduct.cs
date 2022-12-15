using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace AppStoreIntegrationServiceManagement.Model.Plugins
{
    public class RequiredProduct
    {
        private string _minimumStudioVersion;
        private string _maximumStudioVersion;

        [XmlAttribute(AttributeName = "minversion")]
        public string MinimumStudioVersion
        {
            get
            {
                if (Regex.IsMatch(_minimumStudioVersion, @"^(\d{1,2}\.)?(\d{1})$"))
                {
                    return $"{_minimumStudioVersion}.0";
                }

                return _minimumStudioVersion;
            }
            set
            {
                _minimumStudioVersion = value;
            }
        }

        [XmlAttribute(AttributeName = "maxversion")]
        public string MaximumStudioVersion
        {
            get
            {
                if (Regex.IsMatch(_maximumStudioVersion, @"^(\d{1,2}\.)?(\d{1})$"))
                {
                    return $"{_maximumStudioVersion}.0";
                }

                return _maximumStudioVersion;
            }
            set
            {
                _maximumStudioVersion = value;
            }
        }
    }
}
