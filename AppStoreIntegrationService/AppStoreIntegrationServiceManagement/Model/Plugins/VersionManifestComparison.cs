namespace AppStoreIntegrationServiceManagement.Model.Plugins
{
    public class VersionManifestComparison : IEquatable<VersionManifestComparison>
    {
        public bool IsVersionMatch { get; set; }
        public bool IsMinVersionMatch { get; set; }
        public bool IsMaxVersionMatch { get; set; }
        public bool IsProductMatch { get; set; }

        public bool Equals(VersionManifestComparison other)
        {
            return IsVersionMatch == other?.IsVersionMatch &&
                   IsMinVersionMatch == other?.IsMinVersionMatch &&
                   IsMaxVersionMatch == other?.IsMaxVersionMatch &&
                   IsProductMatch == other?.IsProductMatch;
        }
    }
}
