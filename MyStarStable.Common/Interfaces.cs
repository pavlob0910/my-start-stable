using System;

namespace MyStarStable.Common
{
    public sealed class Interfaces
    {
        private static Lazy<Interfaces> _lazyInstance = new Lazy<Interfaces>(() => new Interfaces());

        public static Interfaces Instance 
        { 
            get { return _lazyInstance.Value; } 
        }

        public static void Reset()
        {
            _lazyInstance = new Lazy<Interfaces>(() => new Interfaces());
        }

        public IFacebook FacebookInterface { get; set; }

        public IHMACSHA256 HMAC256Interface { get; set; }

        public INetworkTester NetworkTesterInterface { get; set; }

        private Interfaces()
        {
            HMAC256Interface = null;
            FacebookInterface = null;
        }
    }
}
