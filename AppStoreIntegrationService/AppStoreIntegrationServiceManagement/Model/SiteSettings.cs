namespace AppStoreIntegrationServiceCore.Model
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
