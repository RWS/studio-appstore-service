using static AppStoreIntegrationServiceCore.Enums;

namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginFilter
    {
        public string Query { get; set; }
        public string StudioVersion { get; set; }
        public string SortOrder { get; set; }
        public SortType SortBy { get; set; }
        public string Price { get; set; }
        public List<int> CategoryId { get; set; }
        public Status Status { get; set; }
        public Status VersionStatus { get; set; }
        public string SupportedProduct { get; set; }
    }
}
