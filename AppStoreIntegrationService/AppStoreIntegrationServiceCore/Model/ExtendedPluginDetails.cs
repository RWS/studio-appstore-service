using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Model
{
    public class ExtendedPluginDetails : PluginDetails
    {
        public bool IsEditMode { get; set; }
        public MultiSelectList CategoryListItems { get; set; }
        public IEnumerable<Comment> Comments { get; set; }
        public IEnumerable<Log> Logs { get; set; }
        public IEnumerable<ParentProduct> Parents { get; set; }

        public static new ExtendedPluginDetails CopyFrom(PluginDetails other)
        {
            return JsonConvert.DeserializeObject<ExtendedPluginDetails>(JsonConvert.SerializeObject(other));
        }
    }
}

