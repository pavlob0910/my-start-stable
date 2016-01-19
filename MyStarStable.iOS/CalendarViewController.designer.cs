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
	[Register ("CalendarViewController")]
	partial class CalendarViewController
	{
		[Outlet]
		UIKit.UIBarButtonItem AddButton { get; set; }

		[Outlet]
		Factorymind.Components.FMCalendar CalendarView { get; set; }

		[Outlet]
		UIKit.UITableView EventTableView { get; set; }

		[Outlet]
		UIKit.UIBarButtonItem RefreshButton { get; set; }

		[Action ("RefreshButtonAction:")]
		partial void RefreshButtonAction (UIKit.UIBarButtonItem sender);

		[Action ("SignOutAction:")]
		partial void SignOutAction (UIKit.UIBarButtonItem sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (CalendarView != null) {
				CalendarView.Dispose ();
				CalendarView = null;
			}

			if (EventTableView != null) {
				EventTableView.Dispose ();
				EventTableView = null;
			}

			if (RefreshButton != null) {
				RefreshButton.Dispose ();
				RefreshButton = null;
			}

			if (AddButton != null) {
				AddButton.Dispose ();
				AddButton = null;
			}
		}
	}
}
