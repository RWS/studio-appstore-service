namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginResponseBase<T>
    {
        public string APIVersion { get; set; }
        public IEnumerable<T> Value { get; set; } = new List<T>();
        public IEnumerable<ProductDetails> Products { get; set; }
        public IEnumerable<ParentProduct> ParentProducts { get; set; }
        public IEnumerable<CategoryDetails> Categories { get; set; }
        public IEnumerable<NameMapping> Names { get; set; }
    }
}
