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
	[Register ("EventDetailsViewController")]
	partial class EventDetailsViewController
	{
		[Outlet]
		UIKit.UIBarButtonItem AcceptButton { get; set; }

		[Outlet]
		UIKit.UITextView AttendeesTextView { get; set; }

		[Outlet]
		UIKit.UILabel ClubEventLabel { get; set; }

		[Outlet]
		UIKit.UIBarButtonItem DeleteButton { get; set; }

		[Outlet]
		UIKit.UITextView DescriptionTextView { get; set; }

		[Outlet]
		UIKit.UIBarButtonItem EditButton { get; set; }

		[Outlet]
		UIKit.UITextView EndDateTextView { get; set; }

		[Outlet]
		UIKit.UIBarButtonItem RejectButton { get; set; }

		[Outlet]
		UIKit.UITextView StartDateTextView { get; set; }

		[Outlet]
		UIKit.UITextView TitleTextView { get; set; }

		[Action ("AcceptButtonAction:")]
		partial void AcceptButtonAction (UIKit.UIBarButtonItem sender);

		[Action ("DeleteButtonAction:")]
		partial void DeleteButtonAction (UIKit.UIBarButtonItem sender);

		[Action ("RejectButtonAction:")]
		partial void RejectButtonAction (UIKit.UIBarButtonItem sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (AttendeesTextView != null) {
				AttendeesTextView.Dispose ();
				AttendeesTextView = null;
			}

			if (ClubEventLabel != null) {
				ClubEventLabel.Dispose ();
				ClubEventLabel = null;
			}

			if (DeleteButton != null) {
				DeleteButton.Dispose ();
				DeleteButton = null;
			}

			if (DescriptionTextView != null) {
				DescriptionTextView.Dispose ();
				DescriptionTextView = null;
			}

			if (EditButton != null) {
				EditButton.Dispose ();
				EditButton = null;
			}

			if (EndDateTextView != null) {
				EndDateTextView.Dispose ();
				EndDateTextView = null;
			}

			if (StartDateTextView != null) {
				StartDateTextView.Dispose ();
				StartDateTextView = null;
			}

			if (TitleTextView != null) {
				TitleTextView.Dispose ();
				TitleTextView = null;
			}

			if (AcceptButton != null) {
				AcceptButton.Dispose ();
				AcceptButton = null;
			}

			if (RejectButton != null) {
				RejectButton.Dispose ();
				RejectButton = null;
			}
		}
	}
}
