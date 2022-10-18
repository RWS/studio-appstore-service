namespace AppStoreIntegrationServiceCore.Model
{
    public class PluginResponse<T>
    {
        public string APIVersion { get; set; }

        public List<T> Value { get; set; }

        public List<ProductDetails> Products { get; set; }

        public List<ParentProduct> ParentProducts { get; set; }
    }
}
