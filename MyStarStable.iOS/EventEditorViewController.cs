using System;
using System.Collections.Generic;
using System.Linq;
using BigTed;
using GoogleAnalytics.iOS;
using Foundation;
using UIKit;
using MyStarStable.Common;
using SharpMobileCode.ModalPicker;

namespace MyStarStable.iOS
{
	public partial class EventEditorViewController : UITableViewController
	{
	    private enum DatePickerType
	    {
	        StartDate,
	        EndDate
	    };

        private class DateTextFieldDelegate : UITextFieldDelegate
        {
            public string DatePickerTitle { get; set; }
            public DatePickerType DateType;

            public Action<UITextField, DatePickerType, string, UIColor> DisplayDatePicker { get; set; }
            public UIColor DatePickerTitleColor { get; set; }

            public override bool ShouldBeginEditing(UITextField textField)
            {
                if (DisplayDatePicker != null)
                {
                    DisplayDatePicker(textField, DateType, DatePickerTitle, DatePickerTitleColor);
                    return false;
                }
                return base.ShouldBeginEditing(textField);
            }
        }

        private class AttendeeTextViewDelegate : UITextViewDelegate
        {
            public EventEditorViewController ParentController { get; set; }

            public override bool ShouldBeginEditing(UITextView textView)
            {
                //ParentController.PerformSegue("EventEditorToFriendsSegue", ParentController);
                return false;
            }
        }

        private class DescriptionTextViewDelegate : LimitTextViewDelegate
        {
            public EventEditorViewController ParentController { get; set; }

            public override void Changed(UITextView textView)
            {
                ParentController.Event.Description = textView.Text;
            }
        }

        private readonly ControllerKeyboardHandler _keyboardHandler;
        public CalendarEvent Event { get; set; }
        public DateTime SelectedDate { get; set; }

		public EventEditorViewController (IntPtr handle) : base (handle)
		{
            _keyboardHandler = new ControllerKeyboardHandler(this);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (ClubEventSwitch.On)
            {
                throw new NotImplementedException();
            }

            if (Event == null)
            {
                Event = new CalendarEvent();
            }

            _keyboardHandler.DismissKeyboardOnBackgroundTap();

            AttendeesTextView.Delegate = new AttendeeTextViewDelegate()
            {
                ParentController = this
            };

            StartDateTextField.Delegate = new DateTextFieldDelegate()
            {
                DateType = DatePickerType.StartDate,
                DatePickerTitle = "APP/CAL/PICK_START_DATE",
                DatePickerTitleColor = UIColor.Green,
                DisplayDatePicker = PresentDatePicker
            };
            
            EndDateTextField.Delegate = new DateTextFieldDelegate()
            {
                DateType = DatePickerType.EndDate,
                DatePickerTitle = "APP/CAL/PICK_END_DATE",
                DatePickerTitleColor = UIColor.Red,
                DisplayDatePicker = PresentDatePicker
            };

            TitleTextField.Delegate = new LimitTextFieldDelegate()
            {
                MaxTextLength = 128
            };

            DescriptionTextView.Delegate = new DescriptionTextViewDelegate()
            {
                MaxTextLength = 2048,
                ParentController = this
            };
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (Event.EventId == null)
            {
                GAI.SharedInstance.DefaultTracker.Set(GAIConstants.ScreenName, "Calendar Screen / Create");
                GAI.SharedInstance.DefaultTracker.Send(GAIDictionaryBuilder.CreateScreenView().Build());

                if (Event.StartDate == DateTime.MinValue || Event.EndDate == DateTime.MinValue)
                {
                    DateTime utcStartDate = DateTime.UtcNow;
                    if (SelectedDate != DateTime.MinValue)
                    {
                        Event.StartDate = new DateTime(SelectedDate.Year, SelectedDate.Month, SelectedDate.Day,
                            utcStartDate.Hour, utcStartDate.Minute, utcStartDate.Second, utcStartDate.Millisecond,
                            DateTimeKind.Utc);
                    }
                    else
                    {
                        Event.StartDate = utcStartDate;
                    }
                    Event.EndDate = Event.StartDate.AddHours(1);
                }
            }
            else
            {
                GAI.SharedInstance.DefaultTracker.Set(GAIConstants.ScreenName, "Calendar Screen / Edit");
                GAI.SharedInstance.DefaultTracker.Send(GAIDictionaryBuilder.CreateScreenView().Build());
            }
            AddOwnerAttendee();

            DateTime startLocalTime = Event.StartDate.ToLocalTime();
            DateTime endLocalTime = Event.EndDate.ToLocalTime();

            TitleTextField.Text = Event.Title;
            DescriptionTextView.Text = Event.Description;
            StartDateTextField.Text = startLocalTime.ToShortDateString() + " " + startLocalTime.ToShortTimeString();
            EndDateTextField.Text = endLocalTime.ToShortDateString() + " " + endLocalTime.ToShortTimeString();
            ClubEventSwitch.On = (Event.ClubId != null);
            ClubEventSwitch.Enabled = StableSession.Instance.CurrentUserData.Character.CanCreateClubEvents();
            UpdateAttendeeLabel();
        }

	    private void UpdateAttendeeLabel()
	    {
            string selectedString = Event.Attendees.Aggregate("", (current, attendee) => current + (attendee.FirstName + " " + attendee.LastName + ", "));
	        AttendeesTextView.Text = selectedString.TrimEnd(new char[] { ' ', ',' });
        }

        private void PresentDatePicker(UITextField sender, DatePickerType pickerType, string pickerTitle, UIColor pickerTitleColor)
	    {
            DateTime selectedTime = DateTime.Parse(sender.Text);
            NSDate selectedDate = DateTimeToNSDate(selectedTime);

            var modalPicker = new ModalPickerViewController(ModalPickerType.Date, pickerTitle.Localize(), this)
            {
                HeaderBackgroundColor = pickerTitleColor,
                HeaderTextColor = UIColor.White,
                TransitioningDelegate = new ModalPickerTransitionDelegate(),
                ModalPresentationStyle = UIModalPresentationStyle.Custom
            };
            modalPicker.DatePicker.Date = selectedDate;

            modalPicker.OnModalPickerDismissed += (o, args) =>
            {
                DateTime utcDateTime = NSDateToDateTime(modalPicker.DatePicker.Date);
                if (pickerType == DatePickerType.StartDate)
                {
                    Event.StartDate = utcDateTime;
                }
                else if (pickerType == DatePickerType.EndDate)
                {
                    Event.EndDate = utcDateTime;
                }
                DateTime localDateTime = utcDateTime.ToLocalTime();
                sender.Text = localDateTime.ToShortDateString() + " " + localDateTime.ToShortTimeString();
            };
            PresentViewController(modalPicker, true, null);
	    }

	    private NSDate DateTimeToNSDate(DateTime date)
	    {
            DateTime referenceTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(2001, 1, 1, 0, 0, 0));
            return NSDate.FromTimeIntervalSinceReferenceDate((date - referenceTime).TotalSeconds);
	    }

	    private DateTime NSDateToDateTime(NSDate date)
	    {
            return new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(date.SecondsSinceReferenceDate);
	    }

	    private void AddOwnerAttendee()
	    {
            if (Event.Attendees.Find(attendee => attendee.CharacterId == StableSession.Instance.CurrentUserData.Character.Id) != null)
                return;

            Event.Attendees.Add(new CalendarEventAttendee()
            {
                CharacterId = StableSession.Instance.CurrentUserData.Character.Id,
                FirstName = StableSession.Instance.CurrentUserData.Character.FirstName,
                LastName = StableSession.Instance.CurrentUserData.Character.LastName,
                Status = CalendarEventAttendee.AttendeeStatus.Owner,
            });	        
	    }

	    partial void ClubEventSwitchChanged(UISwitch sender)
	    {
	        AttendeesTextView.Editable = !sender.On;
            AttendeesTextView.Alpha = AttendeesTextView.Editable ? 1.0f : 0.3f;
	        Event.ClubId = (sender.On ? StableSession.Instance.CurrentUserData.Character.GetClubId() : null);
	    }

        partial void TitleTextFieldChanged(UITextField sender)
        {
            Event.Title = sender.Text;
        }

	    partial void UnwindToEditorSegue(UIStoryboardSegue segue)
	    {
	        FriendPickerViewController friendsController = segue.SourceViewController as FriendPickerViewController;
	        if (friendsController != null)
	        {
                Event.Attendees.Clear();

	            HashSet<CharacterFriend> selectedFriends = friendsController.GetSelected();

	            foreach (CharacterFriend friend in selectedFriends)
	            {
                    Event.Attendees.Add(new CalendarEventAttendee()
                    {
                        CharacterId = friend.CharacterId, 
                        FirstName = friend.FirstName, 
                        LastName = friend.LastName,
                        Status = null,
                    });
	            }

                UpdateAttendeeLabel();
	        }
	    }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);

            if (segue.Identifier == "EventEditorToFriendsSegue")
            {
                FriendPickerViewController controller = segue.DestinationViewController as FriendPickerViewController;
                if (controller != null)
                {
                    foreach (CalendarEventAttendee attendee in Event.Attendees)
                    {
                        controller.PreSelected.Add(attendee.CharacterId);
                    }
                    controller.Mode = FriendPickerViewController.SelectionMode.Multiple;
                }
            }
        }

        partial void DoneButtonAction(UIBarButtonItem sender)
        {
            if (!(Event.Title.Length > 0))
            {
                BTProgressHUD.ShowToast("APP/CAL/NEED_TITLE".Localize(), ProgressHUD.MaskType.None, false, 3000);
                return;
            }

            if (Event.Title.Length > 128)
            {
                BTProgressHUD.ShowToast("APP/CAL/TITLE_TOO_LONG".Localize(), ProgressHUD.MaskType.None, false, 3000);
                return;
            }

            if (Event.Description.Length > 2048)
            {
                BTProgressHUD.ShowToast("APP/CAL/DESC_TOO_LONG".Localize(), ProgressHUD.MaskType.None, false, 3000);
                return;
            }

            if (Event.StartDate >= Event.EndDate)
            {
                BTProgressHUD.ShowToast("APP/CAL/START_BEFORE_END".Localize(), ProgressHUD.MaskType.None, false, 3000);
                return;
            }

            if (Event.EventId == null)
            {
                BTProgressHUD.ShowContinuousProgress("APP/CAL/ADDING_EVENT".Localize(), ProgressHUD.MaskType.Black);
                CalendarHelper.Instance.AddEventAsync(Event, AddEventResult);
            }
            else
            {
                BTProgressHUD.ShowContinuousProgress("APP/CAL/EDITING_EVENT".Localize(), ProgressHUD.MaskType.Black);
                CalendarHelper.Instance.EditEventAsync(Event, EditEventResult);
            }
        }

	    private void EditEventResult(CalendarHelper.ModifyEventResult modifyEventResult, string reason, string filteredTitle, string filteredDescription)
	    {
            InvokeOnMainThread(() =>
            {
                BTProgressHUD.Dismiss();
                if (modifyEventResult == CalendarHelper.ModifyEventResult.Ok)
                {
                    NavigationController.PopViewController(true);
                    BTProgressHUD.ShowSuccessWithStatus("APP/CAL/EDIT_SUCCESS".Localize(), 2000);
                }
                else
                {
                    if (modifyEventResult == CalendarHelper.ModifyEventResult.Filtered)
                    {
                        Event.Title = TitleTextField.Text = filteredTitle;
                        Event.Description = DescriptionTextView.Text = filteredDescription;
                        BTProgressHUD.ShowToast("APP/CAL/CRISP_FILTERED".Localize(), ProgressHUD.MaskType.None, false, 3000);
                    }
                    else
                    {
                        UIAlertView alertView = new UIAlertView("APP/ALERT/TITLE/ERROR".Localize(), reason.Localize(), null, "APP/ALERT/BUTTON/OK".Localize(), null);
                        alertView.Show();
                    }
                }
            });
        }

	    private void AddEventResult(CalendarHelper.ModifyEventResult modifyEventResult, string reason, string filteredTitle, string filteredDescription)
	    {
	        InvokeOnMainThread(() =>
	        {
                BTProgressHUD.Dismiss();
	            if (modifyEventResult == CalendarHelper.ModifyEventResult.Ok)
	            {
	                NavigationController.PopViewController(true);
                    BTProgressHUD.ShowSuccessWithStatus("APP/CAL/ADD_SUCCESS".Localize(), 2000);
	            }
	            else
	            {
	                if (modifyEventResult == CalendarHelper.ModifyEventResult.Filtered)
	                {
                        Event.Title = TitleTextField.Text = filteredTitle;
                        Event.Description = DescriptionTextView.Text = filteredDescription;
                        BTProgressHUD.ShowToast("APP/CAL/CRISP_FILTERED".Localize(), ProgressHUD.MaskType.None, false, 3000);
                    }
	                else
	                {
	                    UIAlertView alertView = new UIAlertView("APP/ALERT/TITLE/ERROR".Localize(), reason.Localize(), null, "APP/ALERT/BUTTON/OK".Localize(), null);
	                    alertView.Show();
	                }
	            }
	        });
	    }
	}
}
