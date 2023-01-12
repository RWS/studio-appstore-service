using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Reflection;

namespace AppStoreIntegrationServiceCore.Model
{
    public class ExtendedPluginVersion : PluginVersion
    {
        public ExtendedPluginVersion() { }

        public ExtendedPluginVersion(PluginVersion version)
        {
            PropertyInfo[] properties = typeof(PluginVersion).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                property.SetValue(this, property.GetValue(version));
            }
        }

        [JsonIgnore]
        public int PluginId { get; set; }

        [JsonIgnore]
        [BindProperty]
        public MultiSelectList SupportedProductsListItems { get; set; }

        [JsonIgnore]
        public bool IsNewVersion { get; set; }

        [JsonIgnore]
        public IEnumerable<Comment> VersionComments { get; set; }
    }
}
