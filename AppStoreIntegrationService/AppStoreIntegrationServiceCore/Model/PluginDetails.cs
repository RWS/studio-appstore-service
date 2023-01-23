using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginDetails : PluginDetailsBase<PluginVersion, string>, IEquatable<PluginDetails>
    {
        public Status Status { get; set; }
        public bool IsThirdParty { get; set; }
        public bool NeedsDeletionApproval { get; set; }
        public bool HasAdminConsent { get; set; }
        public bool IsActive { get; set; }
        public bool Equals(PluginDetails other)
        {
            return Name == other.Name &&
                   Description == other.Description &&
                   ChangelogLink == other.ChangelogLink &&
                   SupportUrl == other.SupportUrl &&
                   SupportEmail == other.SupportEmail &&
                   Icon.Equals(other.Icon) &&
                   PaidFor == other.PaidFor &&
                   Developer.Equals(other.Developer) &&
                   Categories.SequenceEqual(other.Categories) &&
                   DownloadUrl == other.DownloadUrl &&
                   Status == other.Status &&
                   IsThirdParty == other.IsThirdParty &&
                   NeedsDeletionApproval == other.NeedsDeletionApproval &&
                   HasAdminConsent == other.HasAdminConsent &&
                   IsActive == other.IsActive;
        }
    }
}
