namespace AppStoreIntegrationServiceManagement.Model.Plugins
{
    public class ManifestCompareProperty
    {
        public string Name { get; set; }
        public string Actual { get; set; }
        public string Expected { get; set; }
        public bool IsMatch { get; set; }
    }
}
