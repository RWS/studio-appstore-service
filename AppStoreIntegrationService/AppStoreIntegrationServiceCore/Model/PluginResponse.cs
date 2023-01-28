namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginResponse<T> : PluginResponseBase<T>, IEquatable<PluginResponse<T>>
    {
        public IEnumerable<T> Pending { get; set; } = new List<T>();
        public IEnumerable<T> Drafts { get; set; } = new List<T>();
        public IDictionary<int, CommentPackage> Comments { get; set; } = new Dictionary<int, CommentPackage>();
        public IDictionary<int, IEnumerable<Log>> Logs { get; set; } = new Dictionary<int, IEnumerable<Log>>();

        public bool Equals(PluginResponse<T> other)
        {
            return Names.SequenceEqual(other?.Names) &&
                   Categories.SequenceEqual(other?.Categories) &&
                   Drafts.SequenceEqual(other?.Drafts) &&
                   Value.SequenceEqual(other?.Value) &&
                   Pending.SequenceEqual(other?.Pending) &&
                   Comments.SequenceEqual(other?.Comments) &&
                   Products.SequenceEqual(other?.Products) &&
                   ParentProducts.SequenceEqual(other?.ParentProducts) &&
                   Logs.SequenceEqual(other?.Logs) &&
                   APIVersion == other?.APIVersion;
        }
    }
}
