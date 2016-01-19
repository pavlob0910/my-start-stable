using System;

namespace MyStarStable.Common
{
    public class Base64
    {
        public static string Encode(byte[] data, bool urlsafe = true)
        {
            if (urlsafe)
            {
                return Convert.ToBase64String(data).Replace("/", "_").Replace("+", "-");
            }
            else
            {
                return Convert.ToBase64String(data);
            }
        }

        public static byte[] Decode(string data, bool urlsafe = true)
        {
            if (urlsafe)
            {
                return Convert.FromBase64String(data.Replace("_", "/").Replace("-", "+"));
            } 
            else
            {
                return Convert.FromBase64String(data);
            }
        }
    }
}
