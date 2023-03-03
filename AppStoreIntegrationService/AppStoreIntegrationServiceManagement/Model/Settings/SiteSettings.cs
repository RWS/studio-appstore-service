namespace AppStoreIntegrationServiceManagement.Model.Settings
{
    public class SiteSettings : IEquatable<SiteSettings>
    {
        public string Name { get; set; }

        public bool Equals(SiteSettings other)
        {
            return Name == other?.Name;
        }
    }
}
