namespace AppStoreIntegrationServiceManagement.Helpers
{
    public class VersionManifestComparison
    {
        public bool IsVersionMatch { get; set; }
        public bool IsMinVersionMatch { get; set; }
        public bool IsMaxVersionMatch { get; set; }
        public bool IsProductMatch { get; set; }
    }
}
