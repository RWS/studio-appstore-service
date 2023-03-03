using Newtonsoft.Json;
using System.Text;

namespace AppStoreIntegrationServiceManagement.Model.Customization
{
    public class CustomizationHelper
    {
        private readonly IEnumerable<string> defaults = new[] { "navbar", "success", "select" };
        private readonly IHttpContextAccessor _context;

        public CustomizationHelper(IHttpContextAccessor context)
        {
            _context = context;
            FontFamilies = JsonConvert.DeserializeObject<List<string>>(Encoding.ASCII.GetString(TemplateResource.FontNames));
            InitFields();
        }

        public List<string> FontFamilies { get; set; } = new();

        public List<CustomizableField> Fields { get; set; }

        public string GetFontSizeForField(string field, string defaultValue)
        {
            var cookies = _context.HttpContext.Request.Cookies;

            if (string.IsNullOrEmpty(field))
            {
                return string.IsNullOrEmpty(cookies["FontSize"]) ? defaultValue : cookies["FontSize"];
            }

            if (string.IsNullOrEmpty(cookies[$"{field}FontSize"]))
            {
                return defaults.Any(x => x == field) && !string.IsNullOrEmpty(cookies["FontSize"]) ? cookies["FontSize"] : defaultValue;
            }

            return cookies[$"{field}FontSize"];
        }

        public string GetForegroundForField(string field, string defaultValue)
        {
            var cookies = _context.HttpContext.Request.Cookies;

            if (string.IsNullOrEmpty(field))
            {
                return string.IsNullOrEmpty(cookies["ForegroundColor"]) ? defaultValue : cookies["ForegroundColor"];
            }

            if (string.IsNullOrEmpty(cookies[$"{field}ForegroundColor"]))
            {
                return defaults.Any(x => x == field) && !string.IsNullOrEmpty(cookies["ForegroundColor"]) ? cookies["ForegroundColor"] : defaultValue;
            }

            return cookies[$"{field}ForegroundColor"];
        }

        public string GetBackgroundForField(string field, string defaultValue)
        {
            var cookies = _context.HttpContext.Request.Cookies;

            if (string.IsNullOrEmpty(field))
            {
                return string.IsNullOrEmpty(cookies["BackgroundColor"]) ? defaultValue : cookies["BackgroundColor"];
            }

            if (string.IsNullOrEmpty(cookies[$"{field}BackgroundColor"]))
            {
                return defaults.Any(x => x == field) && !string.IsNullOrEmpty(cookies["BackgroundColor"]) ? cookies["BackgroundColor"] : defaultValue;
            }

            return cookies[$"{field}BackgroundColor"];
        }

        public string GetFontFamilyForField(string field, string defaultValue)
        {
            var cookies = _context.HttpContext.Request.Cookies;

            if (string.IsNullOrEmpty(field))
            {
                return string.IsNullOrEmpty(cookies["FontFamily"]) ? defaultValue : cookies["FontFamily"];
            }

            if (string.IsNullOrEmpty(cookies[$"{field}FontFamily"]))
            {
                return defaults.Any(x => x == field) && !string.IsNullOrEmpty(cookies["FontFamily"]) ? cookies["FontFamily"] : defaultValue;
            }

            return cookies[$"{field}FontFamily"]?.Replace('+', ' ');
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
