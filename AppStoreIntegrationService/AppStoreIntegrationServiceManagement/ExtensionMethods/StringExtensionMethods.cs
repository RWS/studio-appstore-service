namespace AppStoreIntegrationServiceManagement.ExtensionMethods
{
    public static class StringExtensionMethods
    {
        public static string ToUpperFirst(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            return char.ToUpper(text[0]) + text[1..];
        }
    }
}
