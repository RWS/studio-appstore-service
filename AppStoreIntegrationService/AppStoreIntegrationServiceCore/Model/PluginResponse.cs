namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginResponse<T>
    {
        public string APIVersion { get; set; }
        public IEnumerable<T> Value { get; set; } = new List<T>();
        public IEnumerable<T> Pending { get; set; } = new List<T>();
        public IEnumerable<T> Drafts { get; set; } = new List<T>();
        public IEnumerable<ProductDetails> Products { get; set; }
        public IEnumerable<ParentProduct> ParentProducts { get; set; }
        public IEnumerable<CategoryDetails> Categories { get; set; }
        public IEnumerable<NameMapping> Names { get; set; }
        public IDictionary<int, CommentPackage> Comments { get; set; }
        public IDictionary<int, IEnumerable<Log>> Logs { get; set; }
    }
}
