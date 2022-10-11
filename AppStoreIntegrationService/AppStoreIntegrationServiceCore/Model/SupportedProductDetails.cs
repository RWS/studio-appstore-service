using System.Xml.Linq;

namespace AppStoreIntegrationServiceCore.Model
{
    public class SupportedProductDetails : IEquatable<SupportedProductDetails>
    {
        public string Id { get; set; }
        public string ProductName { get; set; }
        public int? ParentProductID { get; set; }
        public string MinimumStudioVersion { get; set; }
        public bool IsDefault { get; set; }

        public bool Equals(SupportedProductDetails other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (string.IsNullOrEmpty(MinimumStudioVersion) ||
                string.IsNullOrEmpty(other.MinimumStudioVersion))
            {
                return ProductName.Equals(other.ProductName);
            }

            return ProductName.Equals(other.ProductName) &&
                   MinimumStudioVersion.Equals(other.MinimumStudioVersion);
        }

        public override int GetHashCode()
        {
            int hashProductName = ProductName.GetHashCode();
            int hashProductVersion = MinimumStudioVersion == null ? 0 : MinimumStudioVersion.GetHashCode();
            return hashProductName ^ hashProductVersion;
        }
    }
}
