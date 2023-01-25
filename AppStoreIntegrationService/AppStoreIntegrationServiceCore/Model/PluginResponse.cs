namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginResponse<T> : PluginResponseBase<T>
    {
        public IEnumerable<T> Pending { get; set; } = new List<T>();
        public IEnumerable<T> Drafts { get; set; } = new List<T>();
        public IDictionary<int, CommentPackage> Comments { get; set; }
        public IDictionary<int, IEnumerable<Log>> Logs { get; set; }
    }
}
