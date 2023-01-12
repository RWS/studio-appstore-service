using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Reflection;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Model
{
    public class ExtendedPluginDetails : PluginDetails
    {
        public ExtendedPluginDetails() : base() { }

        public ExtendedPluginDetails(PluginDetails other)
        {
            PropertyInfo[] properties = typeof(PluginDetails).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                property.SetValue(this, property.GetValue(other));
            }
        }

        [JsonIgnore]
        public bool IsEditMode { get; set; }
        [JsonIgnore]
        public MultiSelectList CategoryListItems { get; set; }
        [JsonIgnore]
        public string SelectedVersionId { get; set; }
        [JsonIgnore]
        public IEnumerable<Comment> Comments { get; set; }
        public IEnumerable<Log> Logs { get; set; }
        [JsonIgnore]
        public IEnumerable<ParentProduct> Parents { get; set; }
    }
}

