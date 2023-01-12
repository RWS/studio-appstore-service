using Newtonsoft.Json;
using System.Reflection;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginVersion : PluginVersionBase<string>, IEquatable<PluginVersion>
    {
        public bool IsThirdParty { get; set; }
        [JsonProperty("Status")]
        public Status VersionStatus { get; set; }
        public bool NeedsDeletionApproval { get; set; }
        public bool HasAdminConsent { get; set; }

        public bool Equals(PluginVersion other)
        {
            var properties = typeof(PluginVersion).GetProperties().Where(p => !Equals(p.Name, "SupportedProducts"));
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
