using System;
using Foundation;

namespace MyStarStable.iOS
{
    public static class StringExtensions
    {
        public static string Localize(this string translate)
        {
            return NSBundle.MainBundle.LocalizedString(translate, null);
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) 
                return value;

            return value.Substring(0, Math.Min(value.Length, maxLength));
        }
    }
}