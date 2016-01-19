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
	[Register ("EventTableCell")]
	partial class EventTableCell
	{
		[Outlet]
		UIKit.UILabel EndDateLabel { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint SeparatorConstraint { get; set; }

		[Outlet]
		UIKit.UILabel StartDateLabel { get; set; }

		[Outlet]
		UIKit.UILabel TitleLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (EndDateLabel != null) {
				EndDateLabel.Dispose ();
				EndDateLabel = null;
			}

			if (StartDateLabel != null) {
				StartDateLabel.Dispose ();
				StartDateLabel = null;
			}

			if (TitleLabel != null) {
				TitleLabel.Dispose ();
				TitleLabel = null;
			}

			if (SeparatorConstraint != null) {
				SeparatorConstraint.Dispose ();
				SeparatorConstraint = null;
			}
		}
	}
}
