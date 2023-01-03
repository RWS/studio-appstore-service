using AppStoreIntegrationServiceCore.Model;
using System.Diagnostics.CodeAnalysis;

namespace AppStoreIntegrationServiceCore.Comparers
{
    public class IconEqualityComparer : IEqualityComparer<IconDetails>
    {
        public bool Equals(IconDetails x, IconDetails y)
        {
            return x.MediaUrl.Equals(y.MediaUrl);
        }

        public int GetHashCode([DisallowNull] IconDetails icon)
        {
            return icon.MediaUrl.GetHashCode();
        }
    }
}
