using AppStoreIntegrationServiceManagement.Model.Plugins;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceManagement.Model
{
    public class CustomizationHelper
    {
        private readonly IHttpContextAccessor _context;

        public CustomizationHelper(IHttpContextAccessor context)
        {
            _context = context;
            InitFamilies();
            InitFields();
        }

        public SelectList FontFamilies { get; set; }

        public List<CustomizableField> Fields { get; set; }

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

        private void InitFamilies()
        {
            var reader = new RemoteStreamReader(new Uri("https://www.googleapis.com/webfonts/v1/webfonts?key=AIzaSyCD-AbV2jkrj3LOTmLMac6TmILG2EiJciM"));
            var items = JsonConvert.DeserializeObject<FontFamilyResponse>(reader.ReadAsStringAsync().Result).Items;
            var savedFont = _context.HttpContext.Request.Cookies["FontFamilies"];
            FontFamily selectedValue = null;

            if (!string.IsNullOrEmpty(savedFont))
            {
                selectedValue = items.FirstOrDefault(f => f.Family.Equals(savedFont));
            }

            FontFamilies = new SelectList(items, null, nameof(FontFamily.Family), selectedValue);
        }
    }
}
