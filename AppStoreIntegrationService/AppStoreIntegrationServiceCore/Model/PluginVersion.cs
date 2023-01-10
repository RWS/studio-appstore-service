using Newtonsoft.Json;
using System.Reflection;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginVersion<T> : PluginVersionBase<T>, IEquatable<PluginVersion<T>>
    {
        public bool IsThirdParty { get; set; }
        [JsonProperty("Status")]
        public Status VersionStatus { get; set; }
        public bool NeedsDeletionApproval { get; set; }

        public bool Equals(PluginVersion<T> other)
        {
            var properties = typeof(PluginVersion<string>).GetProperties().Where(p => !Equals(p.Name, "SupportedProducts"));
            foreach (PropertyInfo property in properties)
            {
                if (!Equals(property.GetValue(this), property.GetValue(other)))
                {
                    return false;
                }
            }

            return SupportedProducts.SequenceEqual(other.SupportedProducts);
        }
    }
}
