using System.Security.Cryptography;
using MyStarStable.Common;

namespace MyStarStable.iOS
{
    class HMACSHA256_iOS : IHMACSHA256
    {
        public byte[] ComputeHash(byte[] secret, byte[] message)
        {
            HMACSHA256 hmac = new HMACSHA256(secret);
            return hmac.ComputeHash(message);
        }
    }
}