namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginResponseBase<T>
    {
        public string APIVersion { get; set; }
        public IEnumerable<T> Value { get; set; } = new List<T>();
        public IEnumerable<ProductDetails> Products { get; set; } = new List<ProductDetails>();
        public IEnumerable<ParentProduct> ParentProducts { get; set; } = new List<ParentProduct>();
        public IEnumerable<CategoryDetails> Categories { get; set; } = new List<CategoryDetails>();
        public IEnumerable<NameMapping> Names { get; set; } = new List<NameMapping>();
    }
}
