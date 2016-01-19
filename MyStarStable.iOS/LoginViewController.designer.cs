// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace MyStarStable.iOS
{
	[Register ("LoginViewController")]
	partial class LoginViewController
	{
		[Outlet]
		UIKit.UITextField EmailField { get; set; }

		[Outlet]
		UIKit.UIButton FacebookAccountButton { get; set; }

		[Outlet]
		UIKit.UIView LoginView { get; set; }

		[Outlet]
		UIKit.UITextField PasswordField { get; set; }

		[Outlet]
		UIKit.UITextView RegisterDescriptionText { get; set; }

		[Outlet]
		UIKit.UIButton RegisterLinkButton { get; set; }

		[Outlet]
		UIKit.UIView RegisterView { get; set; }

		[Action ("FacebookAccountButton_TouchUpInside:")]
		partial void FacebookAccountButton_TouchUpInside (UIKit.UIButton sender);

		[Action ("FacebookButton_TouchUpInside:")]
		partial void FacebookButton_TouchUpInside (UIKit.UIButton sender);

		[Action ("LogIn_TouchUpInside:")]
		partial void LogIn_TouchUpInside (UIKit.UIButton sender);

		[Action ("Register_TouchUpInside:")]
		partial void Register_TouchUpInside (UIKit.UIButton sender);

		[Action ("RegisterLinkButton_TouchUpInside:")]
		partial void RegisterLinkButton_TouchUpInside (UIKit.UIButton sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (EmailField != null) {
				EmailField.Dispose ();
				EmailField = null;
			}

			if (FacebookAccountButton != null) {
				FacebookAccountButton.Dispose ();
				FacebookAccountButton = null;
			}

			if (LoginView != null) {
				LoginView.Dispose ();
				LoginView = null;
			}

			if (PasswordField != null) {
				PasswordField.Dispose ();
				PasswordField = null;
			}

			if (RegisterDescriptionText != null) {
				RegisterDescriptionText.Dispose ();
				RegisterDescriptionText = null;
			}

			if (RegisterView != null) {
				RegisterView.Dispose ();
				RegisterView = null;
			}

			if (RegisterLinkButton != null) {
				RegisterLinkButton.Dispose ();
				RegisterLinkButton = null;
			}
		}
	}
}
