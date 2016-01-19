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
	[Register ("ComposeViewController")]
	partial class ComposeViewController
	{
		[Outlet]
		UIKit.UITextView MessageTextView { get; set; }

		[Outlet]
		UIKit.UITextField SubjectTextField { get; set; }

		[Outlet]
		UIKit.UITextField ToTextField { get; set; }

		[Action ("SendButtonAction:")]
		partial void SendButtonAction (UIKit.UIBarButtonItem sender);

		[Action ("ToFieldEditingDidBegin:")]
		partial void ToFieldEditingDidBegin (UIKit.UITextField sender);

		[Action ("ToFieldEditingDidEnd:")]
		partial void ToFieldEditingDidEnd (UIKit.UITextField sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (MessageTextView != null) {
				MessageTextView.Dispose ();
				MessageTextView = null;
			}

			if (SubjectTextField != null) {
				SubjectTextField.Dispose ();
				SubjectTextField = null;
			}

			if (ToTextField != null) {
				ToTextField.Dispose ();
				ToTextField = null;
			}
		}
	}
}
