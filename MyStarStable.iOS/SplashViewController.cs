using System;
using GoogleAnalytics.iOS;
using Foundation;
using UIKit;
using MyStarStable.Common;
using Timer = System.Timers.Timer;
using System.Threading;

namespace MyStarStable.iOS
{
	partial class SplashViewController : UIViewController
	{
		/*[Flags]
		private enum LoadingTasksBits : uint
		{
			WebView = 1,
			Login = 2,
			AdTimer = 4,
			All = WebView | Login | AdTimer
		}*/

		//private LoadingTasksBits _loadingTasks = LoadingTasksBits.All;

		//private Timer _adTimer;

		public SplashViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			//_adTimer = new System.Timers.Timer();

			// Width 280, Height 210
			//WebView.LoadFinished += WebViewOnLoadFinished;
			//WebView.LoadError += WebViewOnLoadError;
			//WebView.ScalesPageToFit = true;
			//WebView.ScrollView.ScrollEnabled = false;
//			#if DEBUG
//			WebView.LoadRequest(new NSUrlRequest(new NSUrl("http://dev02.dev.int.starstable.com:7301/static/mystarstable/splash/"), NSUrlRequestCachePolicy.ReloadRevalidatingCacheData, 20));
//			#else
//			WebView.LoadRequest(new NSUrlRequest(new NSUrl("http://hiddenhorse.starstable.com/static/mystarstable/splash/"), NSUrlRequestCachePolicy.ReloadRevalidatingCacheData, 20));
//			#endif//...
			/*       WebView.ShouldStartLoad += (view, request, type) =>
            {
                if (type == UIWebViewNavigationType.LinkClicked)
                {
                    UIApplication.SharedApplication.OpenUrl(request.Url);
                    return false;
                }
                return true;
            };*/
		}
		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);

			RestoreLogin();
		}
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			GAI.SharedInstance.DefaultTracker.Set(GAIConstants.ScreenName, "Splash Screen");
			GAI.SharedInstance.DefaultTracker.Send(GAIDictionaryBuilder.CreateScreenView().Build());

		}

		/*private void OnLoadComplete()
        {
            if (_loadingTasks != 0)
                return;

            InvokeOnMainThread(() =>
            {
                if (!StableSession.Instance.IsActive)
                {
                    PerformSegue("SplashToLoginSegue", this);
                }
                else
                {
                    PerformSegue("SplashToMainSegue", this);
                }
            });
        }*/

		private void RestoreResult(bool result)
		{
			Console.WriteLine("RestoreResult: {0}", result);
			// _loadingTasks &= ~LoadingTasksBits.Login;
			// OnLoadComplete();
			InvokeOnMainThread(() =>
				{
					
					if (!StableSession.Instance.IsActive)//...
					{
						PerformSegue("SplashToLoginSegue", this);
					}
					else
					{
						PerformSegue("SplashToMainSegue", this);
					}

//					PerformSegue("SplashToLoginSegue", this);//...

//					PerformSegue("SplashToMainSegue", this);
				});

		}

		private void DoRestoreLogin()
		{
			StableSession.Instance.RestoreAsync(RestoreResult);
		}

		private void RestoreLogin()
		{
			// We must wait for the reachability interface to be ready.
			NetworkTester_iOS testerIf = Interfaces.Instance.NetworkTesterInterface as NetworkTester_iOS;
			if (testerIf != null && testerIf.Ready)
			{
				DoRestoreLogin();
			}
			else if (testerIf != null)
			{
				testerIf.ReachabilityUpdated += ReachabilityUpdated;
			}
			else
			{
				RestoreResult(false);
			}
		}

		private void ReachabilityUpdated(object sender, EventArgs eventArgs)
		{
			DoRestoreLogin();
		}

		/*private void WebViewOnLoadError(object sender, UIWebErrorArgs uiWebErrorArgs)
        {
            _loadingTasks &= ~LoadingTasksBits.WebView;
            _loadingTasks &= ~LoadingTasksBits.AdTimer;
            OnLoadComplete();
        }*/

		/*private void WebViewOnLoadFinished(object sender, EventArgs eventArgs)
		{
			_loadingTasks &= ~LoadingTasksBits.WebView;

			// Set up fake loading time to display ad.
			_adTimer.Interval = 7000;
			_adTimer.Elapsed += (o, args) =>
			{
				_loadingTasks &= ~LoadingTasksBits.AdTimer;
				OnLoadComplete();
			};
			_adTimer.AutoReset = false;
			_adTimer.Enabled = true;
		}*/
	}
}
