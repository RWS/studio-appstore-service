namespace AppStoreIntegrationServiceCore.ExtensionMethods
{
    public static class DictionaryExtensionMethods
    {
        public static string ToQuery<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return dictionary.Aggregate("", (current, next) => current + $"{next.Key}={next.Value}&");
        }
    }
}
