using System.Security.Cryptography;

namespace AppStoreIntegrationServiceManagement.Helpers
{
    public static class SHA1Generator
    {
        public static string GetHash(Stream stream)
        {
            var shaProvider = SHA1.Create();
            return shaProvider.ComputeHash(stream).ToHexString();
        }

        private static string ToHexString(this byte[] bytes)
        {
            return (from b in bytes select b.ToString("x2")).Aggregate((result, next) => result + next);
        }
    }
}
