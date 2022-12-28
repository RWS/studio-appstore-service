using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceCore.Model
{
    public class ExtendedPluginDetails : PluginDetails<ExtendedPluginVersion, string>
    {
        public ExtendedPluginDetails() : base() { }

        public ExtendedPluginDetails(PluginDetails<PluginVersion<string>, string> other)
        {
            if (other == null)
            {
                return;
            }

            var thisProperties = typeof(PluginDetails<ExtendedPluginVersion, string>).GetProperties();
            var otherProperties = typeof(PluginDetails<PluginVersion<string>, string>).GetProperties();

            for (var i = 0; i < thisProperties.Length; i++)
            {
                if (thisProperties[i].Name != "Versions")
                {
                    thisProperties[i].SetValue(this, otherProperties[i].GetValue(other));
                }
            }

            Versions = other.Versions?.Select(v => new ExtendedPluginVersion(v)).ToList();
        }

        [JsonIgnore]
        public bool IsEditMode { get; set; }
        [JsonIgnore]
        public MultiSelectList CategoryListItems { get; set; }
        [JsonIgnore]
        public string SelectedVersionId { get; set; }
        [JsonIgnore]
        public IEnumerable<Comment> Comments { get; set; }
        [JsonIgnore]
        public IEnumerable<ParentProduct> Parents { get; set; }
    }
}

