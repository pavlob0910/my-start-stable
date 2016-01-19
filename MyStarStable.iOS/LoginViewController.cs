using System;
using BigTed;
using GoogleAnalytics.iOS;
using Foundation;
using UIKit;

using MonoTouch.FacebookConnect;
using MyStarStable.Common;

namespace MyStarStable.iOS
{
	partial class LoginViewController : UIViewController
	{
		private readonly ControllerKeyboardHandler _keyboardHandler;

		public LoginViewController (IntPtr handle) : base (handle)
		{
			_keyboardHandler = new ControllerKeyboardHandler(this);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			//View.AddConstraint (NSLayoutConstraint.Create (View, NSLayoutAttribute.Left, NSLayoutRelation.Equal, LoginView, NSLayoutAttribute.Leading, 1.0f, -16.0f));
			//View.AddConstraint (NSLayoutConstraint.Create (View, NSLayoutAttribute.Right, NSLayoutRelation.Equal, LoginView, NSLayoutAttribute.Trailing, 1.0f, 16.0f));

			_keyboardHandler.InitKeyboardHandling();
			_keyboardHandler.DismissKeyboardOnBackgroundTap();


		}
		public override void ViewDidAppear(bool animated)
		{
			// Hide register elements if visible
			RegisterView.Hidden = true;
			RegisterDescriptionText.Hidden = true;
			RegisterLinkButton.Hidden = true;

			RegisterDescriptionText.Text = "APP/LOGIN/REGISTER/DESCRIPTION".Localize ();
			RegisterLinkButton.SetTitle ("APP/LOGIN/REGISTER/BUTTONTEXT".Localize (), UIControlState.Normal);
		}
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			if (!string.IsNullOrEmpty(NSUserDefaults.StandardUserDefaults.StringForKey ("EMAIL"))) {
				EmailField.Text = NSUserDefaults.StandardUserDefaults.StringForKey ("EMAIL");
				PasswordField.BecomeFirstResponder ();
			}

			GAI.SharedInstance.DefaultTracker.Set(GAIConstants.ScreenName, "Login Screen");
			GAI.SharedInstance.DefaultTracker.Send(GAIDictionaryBuilder.CreateScreenView().Build());

			// This happens when we come from the "sign out" buttons.
			if (StableSession.Instance.IsActive)
			{
				StableSession.Instance.Close(true);
				Interfaces.Instance.FacebookInterface.Close(true);

				MailHelper.Reset();
				CalendarHelper.Reset();
			}

			/*
            FBSession.ActiveSession.SetStateChangeHandler((session, status, error) => InvokeOnMainThread(() =>
            {
                FacebookAccountButton.Enabled = (status == FBSessionState.CreatedOpening ||
                                                 status == FBSessionState.OpenTokenExtended);
            }));
            FacebookAccountButton.Enabled = FBSession.ActiveSession.IsOpen;
            */
		}

		partial void FacebookButton_TouchUpInside(UIButton sender)
		{
			FBSession.ActiveSession = new FBSession(new string[] { "email" });
			FBSession.ActiveSession.Open(FBSessionLoginBehavior.ForcingWebView, FacebookOpenActiveSessionCompleted);
			//FBSession.OpenActiveSession(new string[] {"email"}, true, FacebookOpenActiveSessionCompleted);
		}

		private void HandleFacebookError(NSError error)
		{
			if (FBErrorUtility.ShouldNotifyUser(error))
			{
				string errorText = FBErrorUtility.UserMessage(error);
				UIAlertView alertView = new UIAlertView("APP/ALERT/TITLE/ERROR".Localize(), errorText, null, "APP/ALERT/BUTTON/OK".Localize(), null);
				alertView.Show();
			}
			else
			{
				if (FBErrorUtility.ErrorCategory(error) == FBErrorCategory.UserCancelled)
				{
					Console.WriteLine("Facebook login cancelled by user.");
				}
				else if (FBErrorUtility.ErrorCategory(error) == FBErrorCategory.AuthenticationReopenSession)
				{
					UIAlertView alertView = new UIAlertView("APP/ALERT/TITLE/ERROR".Localize(), "APP/FACEBOOK/ERRORS/RELOGIN".Localize(), null, "APP/ALERT/BUTTON/OK".Localize(), null);
					alertView.Show();
				}
				else
				{
					UIAlertView alertView = new UIAlertView("APP/ALERT/TITLE/ERROR".Localize(), "APP/FACEBOOK/ERRORS/GENERAL".Localize(), null, "APP/ALERT/BUTTON/OK".Localize(), null);
					alertView.Show();
				}
			}
			Interfaces.Instance.FacebookInterface.Close(true);
		}

		private void FacebookOpenActiveSessionCompleted(FBSession session, FBSessionState status, NSError error)
		{
			bool openOk = (error == null && (status == FBSessionState.Open || status == FBSessionState.OpenTokenExtended));

			if (openOk)
			{
				FBRequestConnection.GetMe(FacebookRequestForMeCompleted);
			}
			else if (error != null)
			{
				HandleFacebookError(error);
			}
		}

		private void FacebookRequestForMeCompleted(FBRequestConnection connection, NSObject result, NSError error)
		{
			if (error == null)
			{
				Interfaces.Instance.FacebookInterface.UserEmail = result.ValueForKey(new NSString("email")) as NSString;
				Interfaces.Instance.FacebookInterface.UserId = result.ValueForKey(new NSString("id")) as NSString;
				if (!FBSession.ActiveSession.HasGranted("email"))
				{
					FBSession.ActiveSession.RequestNewReadPermissions(new string[] { "email" }, null);
					UIAlertView alertView = new UIAlertView("APP/ALERT/TITLE/ERROR".Localize(), "APP/FACEBOOK/ERRORS/NEED_EMAIL".Localize(), null, "APP/ALERT/BUTTON/OK".Localize(), null);
					alertView.Show();
					return;
				}
				FacebookLogin();
			}
			else
			{
				HandleFacebookError(error);
			}
		}

		partial void LogIn_TouchUpInside(UIButton sender)
		{
			// Hide register elements if visible
			RegisterView.Hidden = true;
			RegisterDescriptionText.Hidden = true;
			RegisterLinkButton.Hidden = true;

			if (EmailField.Text.Length < 3 || PasswordField.Text.Length <= 0)
			{
				BTProgressHUD.ShowToast("APP/LOGIN/NEED_USER_PASS".Localize(), ProgressHUD.MaskType.None, false, 3000);
				return;
			}

			NSUserDefaults.StandardUserDefaults.SetString(EmailField.Text,"EMAIL");


			StableSession.Instance.LoginAsync(EmailField.Text, PasswordField.Text, LoginResult);
			BTProgressHUD.ShowContinuousProgress("APP/LOGIN/LOGGING_IN".Localize(), ProgressHUD.MaskType.Black);
		}
		partial void Register_TouchUpInside(UIButton sender)
		{
			RegisterView.Hidden = false;
			RegisterDescriptionText.Hidden = false;
			RegisterLinkButton.Hidden = false;
		}

		partial void RegisterLinkButton_TouchUpInside(UIButton sender)
		{
			NSUrl urlToOpen = new NSUrl("APP/LOGIN/REGISTER/LINK".Localize().ToString());
			UIApplication.SharedApplication.OpenUrl (urlToOpen);
		}
		private void FacebookLogin(string user = null, string password = null)
		{
			StableSession.Instance.FacebookLoginAsync(Interfaces.Instance.FacebookInterface.UserId, FBSession.ActiveSession.AccessTokenData.AccessToken, user, password, (result, reason) =>
				{
					if (result == StableSession.LoginResult.Failed)
						InvokeOnMainThread(() => Interfaces.Instance.FacebookInterface.Close(true));
					LoginResult(result, reason);
				});
			BTProgressHUD.ShowContinuousProgress("APP/LOGIN/LOGGING_IN".Localize(), ProgressHUD.MaskType.Black);
		}

		private void LoginResult(StableSession.LoginResult loginResult, string reason)
		{
			InvokeOnMainThread(() =>
				{
					BTProgressHUD.Dismiss();
					if (loginResult == StableSession.LoginResult.Ok)
					{
						PerformSegue("LoginToMainSegue", this);
					}
					else if (loginResult == StableSession.LoginResult.NeedPassword)
					{
						UIAlertView alertView = new UIAlertView("APP/ALERT/TITLE/NEED_PASSWORD".Localize(), "APP/LOGIN/NEED_LINK_PASSWORD".Localize(), null, "APP/ALERT/BUTTON/CANCEL".Localize(), new string[] {"APP/ALERT/BUTTON/OK".Localize()});
						alertView.AlertViewStyle = UIAlertViewStyle.LoginAndPasswordInput;
						alertView.GetTextField(0).Text = Interfaces.Instance.FacebookInterface.UserEmail;
						alertView.Dismissed += LinkPasswordAlertDismissed;
						alertView.Show();
					}
					else
					{
						if (reason.IndexOf("CHARACTER_NOT_FOUND", StringComparison.OrdinalIgnoreCase) >= 0)
						{
							UIAlertView alertView = new UIAlertView("APP/ALERT/TITLE/ERROR".Localize(), "APP/LOGIN/LOGIN_FAILED_CHARACTER".Localize(), null, "APP/ALERT/BUTTON/OK".Localize(), null);
							alertView.Show();
						}
						else
						{
							UIAlertView alertView = new UIAlertView("APP/ALERT/TITLE/ERROR".Localize(), "APP/LOGIN/LOGIN_FAILED".Localize() + reason.Localize(), null, "APP/ALERT/BUTTON/OK".Localize(), null);
							alertView.Show();
						}
					}
				});
		}

		private void LinkPasswordAlertDismissed(object sender, UIButtonEventArgs uiButtonEventArgs)
		{
			UIAlertView view = sender as UIAlertView;
			if (view == null || uiButtonEventArgs.ButtonIndex == 0)
			{
				InvokeOnMainThread(() => Interfaces.Instance.FacebookInterface.Close(true));
				return;
			}
			UITextField userField = view.GetTextField(0);
			UITextField passwordField = view.GetTextField(1);
			if (passwordField == null || passwordField.Text.Length <= 0 || userField == null || userField.Text.Length <= 0)
			{
				InvokeOnMainThread(() => Interfaces.Instance.FacebookInterface.Close(true));
				return;
			}

			FacebookLogin(userField.Text, passwordField.Text);
		}
	}
}
