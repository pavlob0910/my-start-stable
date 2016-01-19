using MonoTouch.FacebookConnect;
using MyStarStable.Common;

namespace MyStarStable.iOS
{
    class Facebook_iOS : IFacebook
    {
        public Facebook_iOS()
        {
            UserId = null;
            UserEmail = null;
        }

        public void Close(bool clearTokens)
        {
            if (!FBSession.ActiveSession.IsOpen)
                return;

            if (clearTokens)
            {
                FBSession.ActiveSession.CloseAndClearTokenInformation();
            }
            FBSession.ActiveSession.Close();
            
            UserId = null;
            UserEmail = null;
        }

        public string UserId { get; set; }
        public string UserEmail { get; set; }
    }
}