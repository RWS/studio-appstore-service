namespace AppStoreIntegrationServiceCore.ExtensionMethods
{
    public static class DictionaryExtensionMethods
    {
        public static string ToQuery<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            string result = string.Empty;

            foreach (var item in dictionary)
            {
                result += $"{item.Key}={item.Value}&";
            }

            return result;
        }
    }
}
