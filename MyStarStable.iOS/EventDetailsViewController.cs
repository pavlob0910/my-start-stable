// This file has been autogenerated from a class added in the UI designer.

using System;
using System.Collections.Generic;
using System.Linq;
using BigTed;
using GoogleAnalytics.iOS;
using UIKit;
using MyStarStable.Common;

namespace MyStarStable.iOS
{
	public partial class EventDetailsViewController : UITableViewController
	{
        public bool InviteDisplay { get; set; }
        public CalendarEvent Event { get; set; }

		public EventDetailsViewController (IntPtr handle) : base (handle)
		{
		    InviteDisplay = false;
		}

	    public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

	        if (InviteDisplay)
	        {
                NavigationItem.RightBarButtonItems = new UIBarButtonItem[] { AcceptButton, RejectButton };
	        }
	        else
	        {
	            NavigationItem.RightBarButtonItems = new UIBarButtonItem[] {EditButton, DeleteButton};
	        }

	        GAI.SharedInstance.DefaultTracker.Set(GAIConstants.ScreenName, "Calendar Screen / View");
            GAI.SharedInstance.DefaultTracker.Send(GAIDictionaryBuilder.CreateScreenView().Build());

            TitleTextView.Text = Event.Title;

            DateTime localStartDate = Event.StartDate.ToLocalTime();
            StartDateTextView.Text = localStartDate.ToShortDateString() + " " + localStartDate.ToShortTimeString();
            
            DateTime localEndDate = Event.EndDate.ToLocalTime();
            EndDateTextView.Text = localEndDate.ToShortDateString() + " " + localEndDate.ToShortTimeString();

            string selectedString = Event.Attendees.Aggregate("", (current, attendee) => current + (attendee.FirstName + " " + attendee.LastName + ", "));
            AttendeesTextView.Text = selectedString.TrimEnd(new char[] { ' ', ',' });

            bool isClubEvent = (Event.ClubId != null && Event.ClubId > 0);
            ClubEventLabel.Text = isClubEvent ? "APP/YES".Localize() : "APP/NO".Localize();

            DescriptionTextView.Text = Event.Description;

            EditButton.Enabled = false;
	        DeleteButton.Enabled = false;

            CalendarEventAttendee me = Event.Attendees.Find(attendee => attendee.CharacterId == StableSession.Instance.CurrentUserData.Character.Id);
            if (me != null)
            {
                if (me.Status == CalendarEventAttendee.AttendeeStatus.Owner ||
                    (isClubEvent && StableSession.Instance.CurrentUserData.Character.CanCreateClubEvents()))
                {
                    EditButton.Enabled = true;
                    DeleteButton.Enabled = true;
                }
            }
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, Foundation.NSObject sender)
        {
            base.PrepareForSegue(segue, sender);

            if (segue.Identifier == "EventDetailsToEditSegue")
            {
                EventEditorViewController controller = segue.DestinationViewController as EventEditorViewController;
                if (controller != null)
                {
                    controller.Event = Event;
                }
            } else if (segue.Identifier == "EventDetailsToAttendeesSegue")
            {
                AttendeeListViewController controller = segue.DestinationViewController as AttendeeListViewController;
                if (controller != null)
                {
                    foreach (CalendarEventAttendee attendee in Event.Attendees)
                    {
                        controller.AddAttendee(attendee);
                    }
                }
            }
        }

        partial void DeleteButtonAction(UIBarButtonItem sender)
        {
            UIAlertView alertView = new UIAlertView("APP/ALERT/TITLE/CONFIRM".Localize(), "APP/CAL/CONFIRM_DELETE".Localize(), null, "APP/ALERT/BUTTON/CANCEL".Localize(), new string[] { "APP/ALERT/BUTTON/OK".Localize() });
            alertView.Dismissed += DeleteAlertDismissed;
            alertView.Show();

        }

        private void DeleteAlertDismissed(object sender, UIButtonEventArgs uiButtonEventArgs)
	    {
            UIAlertView view = sender as UIAlertView;
            if (view == null || uiButtonEventArgs.ButtonIndex == 0)
            {
                return;
            }

            BTProgressHUD.ShowContinuousProgress("APP/CAL/DELETING".Localize(), ProgressHUD.MaskType.Black);
            CalendarHelper.Instance.DeleteEventAsync(Event, DeleteResult);
        }

	    private void DeleteResult(bool result, string reason)
        {
            InvokeOnMainThread(() =>
            {
                BTProgressHUD.Dismiss();
                if (result)
                {
                    BTProgressHUD.ShowToast("APP/CAL/DELETE_SUCCESS".Localize(), ProgressHUD.MaskType.None, false, 3000);
                    NavigationController.PopViewController(true);
                }
                else
                {
                    UIAlertView alertView = new UIAlertView("APP/ALERT/TITLE/ERROR".Localize(), "APP/CAL/DELETE_FAILED".Localize() + reason.Localize(), null, "APP/ALERT/BUTTON/OK".Localize(), null);
                    alertView.Show();
                }
            });
        }

        private void AcceptAlertDismissed(object sender, UIButtonEventArgs uiButtonEventArgs)
        {
            UIAlertView view = sender as UIAlertView;
            if (view == null || uiButtonEventArgs.ButtonIndex == 0)
            {
                return;
            }

            if (Event.EventId != null)
                CalendarHelper.Instance.ReplyInvitationAsync(Event.EventId.Value, true, ReplyResult);
        }

	    partial void AcceptButtonAction(UIBarButtonItem sender)
        {
            UIAlertView alertView = new UIAlertView("APP/ALERT/TITLE/CONFIRM".Localize(), "APP/CAL/CONFIRM_ACCEPT_INVITE".Localize(), null, "APP/ALERT/BUTTON/CANCEL".Localize(), new string[] { "APP/ALERT/BUTTON/OK".Localize() });
            alertView.Dismissed += AcceptAlertDismissed;
            alertView.Show();
        }

        private void RejectAlertDismissed(object sender, UIButtonEventArgs uiButtonEventArgs)
        {
            UIAlertView view = sender as UIAlertView;
            if (view == null || uiButtonEventArgs.ButtonIndex == 0)
            {
                return;
            }

            if (Event.EventId != null)
                CalendarHelper.Instance.ReplyInvitationAsync(Event.EventId.Value, false, ReplyResult);
        }

        partial void RejectButtonAction(UIBarButtonItem sender)
        {
            UIAlertView alertView = new UIAlertView("APP/ALERT/TITLE/CONFIRM".Localize(), "APP/CAL/CONFIRM_REJECT_INVITE".Localize(), null, "APP/ALERT/BUTTON/CANCEL".Localize(), new string[] { "APP/ALERT/BUTTON/OK".Localize() });
            alertView.Dismissed += RejectAlertDismissed;
            alertView.Show();
        }


        private void ReplyResult(bool result, string reason)
        {
            InvokeOnMainThread(() =>
            {
                if (result)
                {
                    NavigationController.PopViewController(true);
                }
                else
                {
                    UIAlertView alertView = new UIAlertView("APP/ALERT/TITLE/ERROR".Localize(), reason.Localize(), null, "APP/ALERT/BUTTON/OK".Localize(), null);
                    alertView.Show();
                }
            });
        }
	}
}
