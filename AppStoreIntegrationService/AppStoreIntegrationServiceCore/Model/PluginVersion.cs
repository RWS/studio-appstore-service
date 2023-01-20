using Newtonsoft.Json;
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
        public bool WasActive { get; set; }

        public bool Equals(PluginVersion other)
        {
            return VersionNumber == other.VersionNumber &&
                   FileHash == other.FileHash &&
                   SupportedProducts.SequenceEqual(other.SupportedProducts) &&
                   AppHasStudioPluginInstaller == other.AppHasStudioPluginInstaller &&
                   MinimumRequiredVersionOfStudio == other.MinimumRequiredVersionOfStudio &&
                   MaximumRequiredVersionOfStudio == other.MaximumRequiredVersionOfStudio &&
                   IsNavigationLink == other.IsNavigationLink &&
                   DownloadUrl == other.DownloadUrl &&
                   IsPrivatePlugin == other.IsPrivatePlugin &&
                   IsThirdParty == other.IsThirdParty &&
                   VersionStatus == other.VersionStatus &&
                   NeedsDeletionApproval == other.NeedsDeletionApproval &&
                   HasAdminConsent == other.HasAdminConsent;
        }
    }
}
