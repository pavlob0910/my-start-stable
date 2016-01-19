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
	[Register ("SettingsViewController")]
	partial class SettingsViewController
	{
		[Outlet]
		UIKit.UIButton blogButton { get; set; }

		[Outlet]
		UIKit.UIButton changePasswordButton { get; set; }

		[Outlet]
		UIKit.UIButton editProfileButton { get; set; }

		[Outlet]
		UIKit.UIButton logoutButton { get; set; }

		[Outlet]
		UIKit.UIButton privacyPolicyButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (blogButton != null) {
				blogButton.Dispose ();
				blogButton = null;
			}

			if (changePasswordButton != null) {
				changePasswordButton.Dispose ();
				changePasswordButton = null;
			}

			if (editProfileButton != null) {
				editProfileButton.Dispose ();
				editProfileButton = null;
			}

			if (logoutButton != null) {
				logoutButton.Dispose ();
				logoutButton = null;
			}

			if (privacyPolicyButton != null) {
				privacyPolicyButton.Dispose ();
				privacyPolicyButton = null;
			}
		}
	}
}
