namespace AppStoreIntegrationServiceCore.Model
{
    public class ParentProduct : IEquatable<ParentProduct>
    {
        public string ParentId { get; set; }

        public string ParentProductName { get; set; }

        public bool Equals(ParentProduct other)
        {
            return ParentProductName == other.ParentProductName;
        }

        public bool IsValid()
        {
            return ParentId != null && ParentProductName != null;
        }
    }
}
