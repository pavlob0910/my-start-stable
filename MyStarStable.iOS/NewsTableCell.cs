using System;
using Foundation;
using MyStarStable.Common;
using UIKit;
using System.Collections.Generic;

namespace MyStarStable.iOS
{
	public class NewsTableSource : UITableViewSource {

		//string[] TableItems;
		string CellIdentifier = "NewsTableViewCell";
		List<Collection1> TableItems;

		NewsViewController owner;

		public NewsTableSource (List<Collection1> items, NewsViewController owner)
		{	
			TableItems = items;
			this.owner = owner;
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return TableItems.Count;
		}
		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			//UIApplication.SharedApplication.OpenUrl (new NSUrl (TableItems [indexPath.Row].newsheadline.href));
			openWeb(TableItems [indexPath.Row].newsheadline.href);
			tableView.DeselectRow (indexPath, true);
		}
		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell (CellIdentifier);
			string item = TableItems[indexPath.Row].newsheadline.text;

			//---- if there are no cells to reuse, create a new one
			if (cell == null)
			{ cell = new UITableViewCell (UITableViewCellStyle.Default, CellIdentifier); }
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			cell.TextLabel.Text = item;

			return cell;
		}
		private void openWeb(string URL) {


			WebViewController myController = owner.Storyboard.InstantiateViewController("WebViewController") as WebViewController;

			if (myController != null)
			{
				myController.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;
				myController.webURL = URL;
				owner.NavigationController.PushViewController (myController, true);
			}


		}
	}
}

