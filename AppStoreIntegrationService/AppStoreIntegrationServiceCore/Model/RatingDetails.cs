using System.Reflection;

namespace AppStoreIntegrationServiceCore.Model
{
    public class RatingDetails : IEquatable<RatingDetails>
    {
        public decimal RatingsCount { get; set; }
        public decimal AverageOverallRating { get; set; }
        public decimal AverageEaseOfUse { get; set; }
        public decimal AverageValue { get; set; }
        public decimal AverageSupport { get; set; }

        public bool Equals(RatingDetails other)
        {
            foreach (PropertyInfo property in typeof(RatingDetails).GetProperties())
            {
                if (!Equals(property.GetValue(this), property.GetValue(other)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
