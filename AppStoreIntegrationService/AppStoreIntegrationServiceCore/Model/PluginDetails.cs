namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginDetails<T, U>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ChangelogLink { get; set; }
        public string SupportUrl { get; set; }
        public string SupportEmail { get; set; }
        public IconDetails Icon { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int DownloadCount { get; set; }
        public int CommentCount { get; set; }
        public string SupportText { get; set; }
        public bool PaidFor { get; set; }
        public bool Inactive { get; set; }
        public string Pricing { get; set; }
        public RatingDetails RatingSummary { get; set; }
        public DeveloperDetails Developer { get; set; }
        public List<IconDetails> Media { get; set; }
        public List<T> Versions { get; set; }
        public List<U> Categories { get; set; }
        public string DownloadUrl { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
