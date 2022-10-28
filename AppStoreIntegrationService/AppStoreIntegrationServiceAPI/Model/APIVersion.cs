using System.Text.RegularExpressions;

namespace AppStoreIntegrationServiceAPI.Model
{
    public class APIVersion
    {
        public int Major { get; set; }

        public int Minor { get; set; }

        public int Patch { get; set; }

        public bool IsVersion(int major, int minor, int patch)
        {
            return Major == major && 
                   Minor == minor && 
                   Patch == patch;
        }

        public static bool TryParse(string value, out APIVersion version)
        {
            var regex = new Regex(@"^(\d+\.)?(\d+\.)?(\d+)$");
            if (value == null || !regex.IsMatch(value))
            {
                version = null;
                return false;
            }

            var digits = value.Split('.');
            version = new APIVersion
            {
                Major = int.Parse(digits[0]),
                Minor = int.Parse(digits[1]),
                Patch = int.Parse(digits[2])
            };
            return true;
        }
    }
}
