using AppStoreIntegrationServiceCore;
using Newtonsoft.Json;
using System.Text;

namespace AppStoreIntegrationServiceManagement.Model
{
    public class CustomizationHelper
    {
        private readonly IEnumerable<string> defaults = new[] { "navbar", "success", "select" };
        private readonly IRequestCookieCollection _cookies;

        public CustomizationHelper(IHttpContextAccessor context)
        {
            _cookies = context.HttpContext.Request.Cookies;
            FontFamilies = JsonConvert.DeserializeObject<List<string>>(Encoding.ASCII.GetString(ServiceResource.FontNames));
            InitFields();
        }

        public List<string> FontFamilies { get; set; } = new ();

        public List<CustomizableField> Fields { get; set; }

        public string GetFontSizeForField(string field, string defaultValue)
        {
            if (string.IsNullOrEmpty(field))
            {
                return string.IsNullOrEmpty(_cookies["FontSize"]) ? defaultValue : _cookies["FontSize"];
            }

            if (string.IsNullOrEmpty(_cookies[$"{field}FontSize"]))
            {
                return defaults.Any(x => x == field) && !string.IsNullOrEmpty(_cookies["FontSize"]) ? _cookies["FontSize"] : defaultValue;
            }

            return _cookies[$"{field}FontSize"];
        }

        public string GetForegroundForField(string field, string defaultValue)
        {
            if (string.IsNullOrEmpty(field))
            {
                return string.IsNullOrEmpty(_cookies["ForegroundColor"]) ? defaultValue : _cookies["ForegroundColor"];
            }

            if (string.IsNullOrEmpty(_cookies[$"{field}ForegroundColor"]))
            {
                return defaults.Any(x => x == field) && !string.IsNullOrEmpty(_cookies["ForegroundColor"]) ? _cookies["ForegroundColor"] : defaultValue;
            }

            return _cookies[$"{field}ForegroundColor"];
        }

        public string GetBackgroundForField(string field, string defaultValue)
        {
            if (string.IsNullOrEmpty(field))
            {
                return string.IsNullOrEmpty(_cookies["BackgroundColor"]) ? defaultValue : _cookies["BackgroundColor"];
            }

            if (string.IsNullOrEmpty(_cookies[$"{field}BackgroundColor"]))
            {
                return defaults.Any(x => x == field) && !string.IsNullOrEmpty(_cookies["BackgroundColor"]) ? _cookies["BackgroundColor"] : defaultValue;
            }

            return _cookies[$"{field}BackgroundColor"];
        }

        public string GetFontFamilyForField(string field, string defaultValue)
        {
            if (string.IsNullOrEmpty(field))
            {
                return string.IsNullOrEmpty(_cookies["FontFamily"]) ? defaultValue : _cookies["FontFamily"];
            }

            if (string.IsNullOrEmpty(_cookies[$"{field}FontFamily"]))
            {
                return defaults.Any(x => x == field) && !string.IsNullOrEmpty(_cookies["FontFamily"]) ? _cookies["FontFamily"] : defaultValue;
            }

            return _cookies[$"{field}FontFamily"]?.Replace('+', ' ');
        }

        private void InitFields()
        {
            Fields = new List<CustomizableField> 
            {
                new CustomizableField{ Name="Navbar", Tag = "navbar", DefaultBackground = "#28b059", DefaultForeground = "#FFFFFF" },
                new CustomizableField{ Name="Modal", Tag = "modal", DefaultBackground = "#FFFFFF", DefaultForeground = "#000000" },
                new CustomizableField{ Name="Page", Tag = "body", DefaultBackground = "#FFFFFF", DefaultForeground = "#000000" },
                new CustomizableField{ Name="Dropdown", Tag = "select", DefaultBackground = "#28b059", DefaultForeground = "#FFFFFF" },
                new CustomizableField{ Name="Plugin tile", Tag = "card", DefaultBackground = "#FFFFFF", DefaultForeground = "#000000" },
                new CustomizableField{ Name="Table", Tag = "table", DefaultBackground = "#f2f2f2", DefaultForeground = "#000000" },
                new CustomizableField{ Name="Input", Tag = "input", DefaultBackground = "#FFFFFF", DefaultForeground = "#000000" },
                new CustomizableField{ Name="Success button", Tag = "success", DefaultBackground = "#28b059", DefaultForeground = "#FFFFFF" },
                new CustomizableField{ Name="Secondary button", Tag = "secondary", DefaultBackground = "#6c757d", DefaultForeground = "#FFFFFF" },
                new CustomizableField{ Name="Danger button", Tag = "danger", DefaultBackground = "#dc3545", DefaultForeground = "#FFFFFF" }
            };
        }
    }
}
