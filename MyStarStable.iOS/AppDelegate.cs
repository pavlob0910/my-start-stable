using System;
using System.Runtime.InteropServices;
using GoogleAnalytics.iOS;
using Foundation;
using UIKit;
using System.Drawing;

using MonoTouch.FacebookConnect;
using MyStarStable.Common;

namespace MyStarStable.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        public override UIWindow Window
        {
            get;
            set;
        }

        public int InitialTabTag { get; private set; }

        private const string FacebookAppId = "1479131262375302";
        private const string DisplayName = "My Star Stable";

        private const string GoogleAnalyticsId = "UA-20083095-8";
        public IGAITracker GoogleTracker;

		public static UIStoryboard Storyboard = UIStoryboard.FromName ("MainStoryboard", null);
		public static UIViewController initialViewController;

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            InitialTabTag = 4; //-1 was orginal

            StableSession.ApiGuid = "13e723d7763b494db92b13fc05c40214";
            StableSession.DeviceId = UIDevice.CurrentDevice.IdentifierForVendor.AsString();

            FBSettings.DefaultAppID = FacebookAppId;
            FBSettings.DefaultDisplayName = DisplayName;

#if DEBUG
            // Disable analytics when running in debug.
            GAI.SharedInstance.DryRun = true;
#endif
            GAI.SharedInstance.DispatchInterval = 20;
            GAI.SharedInstance.TrackUncaughtExceptions = true;
            GoogleTracker = GAI.SharedInstance.GetTracker(GoogleAnalyticsId);
            GoogleTracker.SetAllowIdfaCollection(true);

            Interfaces.Instance.HMAC256Interface = new HMACSHA256_iOS();
            Interfaces.Instance.FacebookInterface = new Facebook_iOS();
            Interfaces.Instance.NetworkTesterInterface = new NetworkTester_iOS();

            
            // Register for push notifications
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                const UIUserNotificationType notificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound;
                var settings = UIUserNotificationSettings.GetSettingsForTypes(notificationTypes, new NSSet());
                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            }
            else
            {
                const UIRemoteNotificationType notificationTypes = UIRemoteNotificationType.Alert | UIRemoteNotificationType.Badge | UIRemoteNotificationType.Sound;
                UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(notificationTypes);
            }

            if (options != null)
            {
                if (options.ContainsKey(UIApplication.LaunchOptionsRemoteNotificationKey))
                {
                    NSDictionary userInfo = options[UIApplication.LaunchOptionsRemoteNotificationKey] as NSDictionary;
                    if (userInfo != null)
                    {
                        NSDictionary aps = userInfo["aps"] as NSDictionary;
                        if (aps != null)
                        {
                            if (aps.ContainsKey(new NSString("mail_id")))
                            {
                                InitialTabTag = 1;
                            }
                            else if (aps.ContainsKey(new NSString("event_id")))
                            {
                                InitialTabTag = 2;
                            }
                        }
                    }
                }
            }

            MailHelper.Instance.UnreadCountChanged += MailHelperOnUnreadCountChanged;
            CalendarHelper.Instance.PendingEventsChanged += CalendarHelperOnPendingEventsChanged;

			// Här sätter vi central navigeringsfärg för hela appen till rosa färgen och vit text
			UINavigationBar.Appearance.BarTintColor = new UIColor (0.357f, 0.812f , 0.996f, 1.00f);//...
			//			UINavigationBar.Appearance.BarTintColor = UIColor.FromRGBA (20, 54, 109, 1);
			UINavigationBar.Appearance.TintColor = UIColor.White;
			UINavigationBar.Appearance.SetTitleTextAttributes(new UITextAttributes()
				{

					TextColor = UIColor.White
				});
			
            return true;
        }

        private void CalendarHelperOnPendingEventsChanged(object sender, EventArgs eventArgs)
        {
            UpdateBadgeCount();
        }

        private void MailHelperOnUnreadCountChanged(object sender, MailHelper.UnreadCountChangedEventArgs e)
        {
            UpdateBadgeCount();
        }

        private void UpdateBadgeCount()
        {
            InvokeOnMainThread(() =>
            {
                int pendingCount = CalendarHelper.Instance.PendingEvents != null ? CalendarHelper.Instance.PendingEvents.Count : 0;
                UIApplication.SharedApplication.ApplicationIconBadgeNumber = MailHelper.Instance.UnreadCount + pendingCount;
            });
        }

        public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
            NSDictionary aps = userInfo["aps"] as NSDictionary;
            if (aps != null)
            {
                if (aps.ContainsKey(new NSString("mail_id")))
                {
                    MailHelper.Instance.GetMailAsync(null);
                }
                else if (aps.ContainsKey(new NSString("event_id")))
                {
                    CalendarHelper.Instance.GetPendingEventsAsync(null);
                }
            }
        }

        /// <summary>
        /// Finished registering user notification settings (ios 8+ only).
        /// </summary>
        public override void DidRegisterUserNotificationSettings(UIApplication application, UIUserNotificationSettings notificationSettings)
        {
            UIApplication.SharedApplication.RegisterForRemoteNotifications();
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            InvokeOnMainThread(() =>
            {
                StableSession.DevicePushToken = new byte[deviceToken.Length];
                Marshal.Copy(deviceToken.Bytes, StableSession.DevicePushToken, 0, Convert.ToInt32(deviceToken.Length));
            });
            //Common.StableSession.Instance.DevicePushToken = deviceToken.ToString();
        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            InvokeOnMainThread(() =>
            {
                StableSession.DevicePushToken = null;
            });
        }

        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            // We need to handle URLs by passing them to FBSession in order for SSO authentication
            // to work.
            return FBSession.ActiveSession.HandleOpenURL(url);
        }

        public override void OnActivated(UIApplication application)
        {
            // We need to properly handle activation of the application with regards to SSO
            // (e.g., returning from iOS 6.0 authorization dialog or from fast app switching).
            FBSession.ActiveSession.HandleDidBecomeActive();

            FBAppEvents.ActivateApp();
        }

        //
        // This method is invoked when the application is about to move from active to inactive state.
        //
        // OpenGL applications should use this method to pause.
        //
        public override void OnResignActivation(UIApplication application)
        {
        }

        // This method should be used to release shared resources and it should store the application state.
        // If your application supports background exection this method is called instead of WillTerminate
        // when the user quits.
        public override void DidEnterBackground(UIApplication application)
        {
        }

        // This method is called as part of the transiton from background to active state.
        public override void WillEnterForeground(UIApplication application)
        {
        }

        // This method is called when the application is about to terminate. Save data, if needed. 
        public override void WillTerminate(UIApplication application)
        {
        }
    }
}