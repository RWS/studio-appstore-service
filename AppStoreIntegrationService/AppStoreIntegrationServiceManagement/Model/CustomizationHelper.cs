using AppStoreIntegrationServiceCore;
using Newtonsoft.Json;
using System.Text;

namespace AppStoreIntegrationServiceManagement.Model
{
    public class CustomizationHelper
    {
        public CustomizationHelper()
        {
            FontFamilies = JsonConvert.DeserializeObject<List<string>>(Encoding.ASCII.GetString(ServiceResource.FontNames));
            InitFields();
        }

        public List<string> FontFamilies { get; set; } = new ();

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
    }
}
