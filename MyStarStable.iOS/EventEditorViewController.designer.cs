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
	[Register ("EventEditorViewController")]
	partial class EventEditorViewController
	{
		[Outlet]
		UIKit.UITextView AttendeesTextView { get; set; }

		[Outlet]
		UIKit.UISwitch ClubEventSwitch { get; set; }

		[Outlet]
		UIKit.UITextView DescriptionTextView { get; set; }

		[Outlet]
		UIKit.UITextField EndDateTextField { get; set; }

		[Outlet]
		UIKit.UITextField StartDateTextField { get; set; }

		[Outlet]
		UIKit.UITextField TitleTextField { get; set; }

		[Action ("ClubEventSwitchChanged:")]
		partial void ClubEventSwitchChanged (UIKit.UISwitch sender);

		[Action ("DoneButtonAction:")]
		partial void DoneButtonAction (UIKit.UIBarButtonItem sender);

		[Action ("TitleTextFieldChanged:")]
		partial void TitleTextFieldChanged (UIKit.UITextField sender);

		[Action ("UnwindToEditorSegue:")]
		partial void UnwindToEditorSegue (UIKit.UIStoryboardSegue segue);
		
		void ReleaseDesignerOutlets ()
		{
			if (AttendeesTextView != null) {
				AttendeesTextView.Dispose ();
				AttendeesTextView = null;
			}

			if (ClubEventSwitch != null) {
				ClubEventSwitch.Dispose ();
				ClubEventSwitch = null;
			}

			if (DescriptionTextView != null) {
				DescriptionTextView.Dispose ();
				DescriptionTextView = null;
			}

			if (EndDateTextField != null) {
				EndDateTextField.Dispose ();
				EndDateTextField = null;
			}

			if (StartDateTextField != null) {
				StartDateTextField.Dispose ();
				StartDateTextField = null;
			}

			if (TitleTextField != null) {
				TitleTextField.Dispose ();
				TitleTextField = null;
			}
		}
	}
}
