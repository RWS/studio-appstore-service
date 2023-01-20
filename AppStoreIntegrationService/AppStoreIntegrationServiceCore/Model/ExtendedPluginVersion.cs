using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Model
{
    public class ExtendedPluginVersion : PluginVersion
    {
        [JsonIgnore]
        public int PluginId { get; set; }

        [JsonIgnore]
        [BindProperty]
        public MultiSelectList SupportedProductsListItems { get; set; }

        [JsonIgnore]
        public bool IsNewVersion { get; set; }

        [JsonIgnore]
        public IEnumerable<Comment> VersionComments { get; set; }

        public static new ExtendedPluginVersion CopyFrom(PluginVersion other)
        {
            var toString = JsonConvert.SerializeObject(other);
            return JsonConvert.DeserializeObject<ExtendedPluginVersion>(toString);
        }
    }
}
