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
	[Register ("MailMessageViewController")]
	partial class MailMessageViewController
	{
		[Outlet]
		UIKit.UILabel AttachmentLabel { get; set; }

		[Outlet]
		UIKit.UITextView FromTextView { get; set; }

		[Outlet]
		UIKit.UITextView MessageTextView { get; set; }

		[Outlet]
		UIKit.UIBarButtonItem ReplyButton { get; set; }

		[Outlet]
		UIKit.UITextView SubjectTextView { get; set; }

		[Outlet]
		UIKit.UIBarButtonItem TrashButton { get; set; }

		[Action ("DeleteButtonAction:")]
		partial void DeleteButtonAction (UIKit.UIBarButtonItem sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (AttachmentLabel != null) {
				AttachmentLabel.Dispose ();
				AttachmentLabel = null;
			}

			if (FromTextView != null) {
				FromTextView.Dispose ();
				FromTextView = null;
			}

			if (MessageTextView != null) {
				MessageTextView.Dispose ();
				MessageTextView = null;
			}

			if (ReplyButton != null) {
				ReplyButton.Dispose ();
				ReplyButton = null;
			}

			if (SubjectTextView != null) {
				SubjectTextView.Dispose ();
				SubjectTextView = null;
			}

			if (TrashButton != null) {
				TrashButton.Dispose ();
				TrashButton = null;
			}
		}
	}
}
