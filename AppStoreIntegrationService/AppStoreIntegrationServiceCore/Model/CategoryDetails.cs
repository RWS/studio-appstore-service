namespace AppStoreIntegrationServiceCore.Model
{
    public class CategoryDetails : IEquatable<CategoryDetails>
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public bool Equals(CategoryDetails other)
        {
            return Name == other.Name;
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Name);
        }
    }
}
