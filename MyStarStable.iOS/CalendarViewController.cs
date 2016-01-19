using System;
using System.Collections.Generic;
using System.Linq;
using BigTed;
using GoogleAnalytics.iOS;
using Foundation;
using UIKit;
using MyStarStable.Common;

namespace MyStarStable.iOS
{
    public class CalendarDataSource : UITableViewSource
    {
        private const string CellIdentifier = "EventTableCell";

        private List<CalendarEvent> _events;

        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get { return _selectedDate; }
            set
            {
                _selectedDate = value;
                ReloadData();
            }
        }

        public CalendarDataSource()
        {
        }

        public void ReloadData()
        {
            _events = CalendarHelper.Instance.GetEventsForDate(_selectedDate);
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            if (_events != null)
                return _events.Count;

            return 0;
        }

        /*
        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            //return base.CanEditRow(tableView, indexPath);
            return true;
        }

        public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
        {
            //return base.EditingStyleForRow(tableView, indexPath);
            return UITableViewCellEditingStyle.Delete;
        }

        public override bool ShouldIndentWhileEditing(UITableView tableView, NSIndexPath indexPath)
        {
            //return base.ShouldIndentWhileEditing(tableView, indexPath);
            return true;
        }
        */

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            EventTableCell cell = tableView.DequeueReusableCell(CellIdentifier) as EventTableCell;
            // if there are no cells to reuse, create a new one
            if (cell == null)
                cell = new EventTableCell(CellIdentifier);
            // Must be first!
            cell.SelectedTime = SelectedDate;
            cell.Event = _events[indexPath.Row];
			cell.BackgroundColor = UIColor.FromRGBA (227, 227, 227, 1); //...
            //CharacterMailMessage message = MailHelper.Instance.Messages[indexPath.Row];
            //cell.MailMessage = message;
            return cell;
        }
    }

    public partial class CalendarViewController : UIViewController
    {
        public CalendarViewController(IntPtr handle) : base(handle)
        {
        }
        
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            GAI.SharedInstance.DefaultTracker.Set(GAIConstants.ScreenName, "Calendar Screen");
            GAI.SharedInstance.DefaultTracker.Send(GAIDictionaryBuilder.CreateScreenView().Build());
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationItem.RightBarButtonItems = new UIBarButtonItem[] { AddButton };

            EventTableView.Source = new CalendarDataSource();
            CalendarView.IsDayMarkedDelegate = time =>
            {
                return CalendarHelper.Instance.DayHasEvent(time);
            };

            CalendarView.DateSelected += time =>
            {
                CalendarDataSource source = EventTableView.Source as CalendarDataSource;
                if (source != null)
                {
                    source.SelectedDate = time;
                    EventTableView.ReloadData();
                }
            };

            CalendarHelper.Instance.EventsChanged += (sender, args) => InvokeOnMainThread(() =>
            {
                CalendarDataSource source = EventTableView.Source as CalendarDataSource;
                if (source != null)
                    source.ReloadData();

                EventTableView.ReloadData();
                CalendarView.SetNeedsDisplay();
            });

            CalendarView.MonthChanged += (oldDate, newDate) =>
            {
                if (newDate > oldDate)
                {
                    DateTime newNextMonth = newDate.AddMonths(1);
                    CalendarHelper.Instance.GetEventsAsync(newNextMonth.Year, newNextMonth.Month, HandleGetEventsError);
                }
                else
                {
                    DateTime newPrevMonth = newDate.AddMonths(-1);
                    CalendarHelper.Instance.GetEventsAsync(newPrevMonth.Year, newPrevMonth.Month, HandleGetEventsError);
                }
            };

            RefreshEvents(b => CalendarView.SelectToday());
        }

        private void RefreshEvents(Action<bool> resultAction)
        {
            DateTime prevMonth = CalendarView.CurrentMonthYear.AddMonths(-1);
            DateTime nextMonth = CalendarView.CurrentMonthYear.AddMonths(1);
            List<StableSession.YearMonth> months = new List<StableSession.YearMonth>
            {
                new StableSession.YearMonth()
                {
                    Year = prevMonth.Year,
                    Month = prevMonth.Month
                },
                new StableSession.YearMonth()
                {
                    Year = CalendarView.CurrentMonthYear.Year,
                    Month = CalendarView.CurrentMonthYear.Month
                },
                new StableSession.YearMonth()
                {
                    Year = nextMonth.Year,
                    Month = nextMonth.Month
                },
            };
            
            CalendarHelper.Instance.InvalidateCache(months);

            CalendarHelper.Instance.GetEventsAsync(months,
                (result, reason) => InvokeOnMainThread(() =>
                {
                    resultAction(result);
                    HandleGetEventsError(result, reason);
                }));            
        }

        private void HandleGetEventsError(bool result, string reason)
        {
            if (!result)
            {
                UIAlertView alertView = new UIAlertView("APP/ALERT/TITLE/ERROR".Localize(), reason.Localize(), null, "APP/ALERT/BUTTON/OK".Localize(), null);
                alertView.Show();
            }
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);

            if (segue.Identifier == "CalendarToEventDetailSegue")
            {
                EventDetailsViewController controller = segue.DestinationViewController as EventDetailsViewController;
                if (controller != null)
                {
                    EventTableCell cell = sender as EventTableCell;
                    if (cell != null)
                    {
                        controller.Event = cell.Event;
                    }
                    controller.InviteDisplay = false;
                }
            }
            else if (segue.Identifier == "CalendarToEventEditorSegue")
            {
                CalendarDataSource source = EventTableView.Source as CalendarDataSource;
                EventEditorViewController controller = segue.DestinationViewController as EventEditorViewController;
                if (controller != null && source != null)
                {
                    controller.SelectedDate = source.SelectedDate;
                }
            }
        }

        private void SignOutAlertDismissed(object sender, UIButtonEventArgs uiButtonEventArgs)
        {
            UIAlertView view = sender as UIAlertView;
            if (view == null || uiButtonEventArgs.ButtonIndex == 0)
            {
                return;
            }

            PerformSegue("SignOutSegue", this);
        }

        partial void SignOutAction(UIBarButtonItem sender)
        {
            UIAlertView alertView = new UIAlertView("APP/ALERT/TITLE/SIGN_OUT".Localize(), "APP/LOGIN/CONFIRM_SIGNOUT".Localize(), null, "APP/ALERT/BUTTON/CANCEL".Localize(), new string[] { "APP/ALERT/BUTTON/OK".Localize() });
            alertView.Dismissed += SignOutAlertDismissed;
            alertView.Show();
        }

        partial void RefreshButtonAction(UIBarButtonItem sender)
        {
            BTProgressHUD.ShowContinuousProgress("APP/CAL/LOADING_EVENTS".Localize(), ProgressHUD.MaskType.Black);
            RefreshEvents(b => BTProgressHUD.Dismiss());
        }
    }
}
