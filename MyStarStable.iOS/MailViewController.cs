using System;
using CoreGraphics;
using GoogleAnalytics.iOS;
using Foundation;
using UIKit;
using MyStarStable.Common;

namespace MyStarStable.iOS
{
    public class MailDataSource : UITableViewSource
    {
        private const string CellIdentifier = "MailTableCell";

        public MailDataSource()
        {
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            if (MailHelper.Instance.Messages != null)
                return MailHelper.Instance.Messages.Count;

            return 0;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            MailTableCell cell = tableView.DequeueReusableCell(CellIdentifier) as MailTableCell;
            // if there are no cells to reuse, create a new one
            if (cell == null)
                cell = new MailTableCell(CellIdentifier);
            CharacterMailMessage message = MailHelper.Instance.Messages[indexPath.Row];
            cell.MailMessage = message;
            return cell;
        }
    }

    public sealed partial class MailViewController : UITableViewController
    {
        public MailViewController(IntPtr handle)
            : base(handle)
        {
            RefreshControl = new UIRefreshControl();
            RefreshControl.ValueChanged += RefreshControlOnValueChanged;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.Source = new MailDataSource();

            MailHelper.Instance.MessagesChanged += MailHelperOnMessagesChanged;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            GAI.SharedInstance.DefaultTracker.Set(GAIConstants.ScreenName, "Mail Screen");
            GAI.SharedInstance.DefaultTracker.Send(GAIDictionaryBuilder.CreateScreenView().Build());

            if (MailHelper.Instance.LoadingMail)
            {
                TableView.ContentOffset = new CGPoint(TableView.ContentOffset.X, TableView.ContentOffset.Y - RefreshControl.Frame.Height);
                RefreshControl.BeginRefreshing();
            }
        }

        private void MailHelperOnMessagesChanged(object sender, EventArgs eventArgs)
        {
            InvokeOnMainThread(() => {
                RefreshControl.EndRefreshing();
                TableView.ReloadData();
            });
        }

        private void RefreshControlOnValueChanged(object sender, EventArgs eventArgs)
        {
            RefreshMails();
        }

        private void RefreshMails()
        {
            RefreshControl.BeginRefreshing();
            MailHelper.Instance.GetMailAsync(GetMailResult);
        }

        private void GetMailResult(bool result, string reason)
        {
            InvokeOnMainThread(() =>
            {
                if (!result)
                {
                    UIAlertView alertView = new UIAlertView("APP/ALERT/TITLE/ERROR".Localize(), "APP/MAIL/FETCH_FAILED".Localize() + reason.Localize(), null, "APP/ALERT/BUTTON/OK".Localize(), null);
                    alertView.Show();
                }
            });
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            base.PrepareForSegue(segue, sender);

            if (segue.Identifier == "MailToMessageSegue")
            {
                MailMessageViewController controller = segue.DestinationViewController as MailMessageViewController;
                MailTableCell mailCell = sender as MailTableCell;
                if (controller != null && mailCell != null)
                {
                    controller.MailMessage = mailCell.MailMessage;
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
    }
}
