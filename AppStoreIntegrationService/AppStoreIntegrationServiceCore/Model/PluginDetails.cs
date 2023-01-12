using System.Reflection;
using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginDetails : PluginDetailsBase<PluginVersion, string>, IEquatable<PluginDetails>
    {
        public Status Status { get; set; }
        public bool IsThirdParty { get; set; }
        public bool NeedsDeletionApproval { get; set; }
        public bool HasAdminConsent { get; set; }
        public bool Equals(PluginDetails other)
        {
            var properties = typeof(PluginDetails).GetProperties().Where(p => !p.Name.Equals("Versions"));

            foreach (PropertyInfo property in properties)
            {
                bool ok = property.Name switch
                {
                    "Categories" => Categories.SequenceEqual(other.Categories),
                    "Icon" => Icon.Equals(other.Icon),
                    "Developer" => Developer.Equals(other.Developer),
                    _ => Equals(property.GetValue(this), property.GetValue(other))
                };

                if (!ok)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
