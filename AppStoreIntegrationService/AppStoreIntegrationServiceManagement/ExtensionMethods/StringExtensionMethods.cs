﻿namespace AppStoreIntegrationServiceManagement.ExtensionMethods
{
    public static class StringExtensionMethods
    {
        public static string ToUpperFirst(this string text)
        {
            return char.ToUpper(text[0]) + text[1..];
        }
    }
}