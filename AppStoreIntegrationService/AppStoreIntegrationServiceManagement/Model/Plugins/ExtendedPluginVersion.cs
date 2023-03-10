using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using AppStoreIntegrationServiceManagement.Model.Comments;
using AppStoreIntegrationServiceCore.Model;

namespace AppStoreIntegrationServiceManagement.Model.Plugins
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
            if (other == null)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<ExtendedPluginVersion>(JsonConvert.SerializeObject(other));
        }
    }
}
