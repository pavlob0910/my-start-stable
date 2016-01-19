using System;

namespace MyStarStable.Common
{
    public class SettingsObject
    {
        public UInt64 AuthorizationTokenId {get; set;}

        public byte[] AuthorizationToken { get; set; }

        public SettingsObject()
        {
            AuthorizationToken = new byte[0];
        }
    }
}
