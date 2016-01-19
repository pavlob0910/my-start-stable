using System;
using UIKit;
using MyStarStable.Common;

namespace MyStarStable.iOS
{
	public partial class EventInvitationTableCell : UITableViewCell
	{
	    private CalendarEvent _event;
	    public CalendarEvent Event
	    {
	        get
	        {
	            return _event;
	        }
	        set
	        {
	            _event = value;

                DateTime localStartDate = _event.StartDate.ToLocalTime();
                TextLabel.Text = String.Format("APP/CAL/EVENT_INVITE_CELL_TITLE".Localize(), localStartDate.ToShortDateString() + " " + localStartDate.ToShortTimeString());
	            DetailTextLabel.Text = _event.Title;
	        }
	    }

	    public EventInvitationTableCell(IntPtr handle) : base(handle)
		{
		}

	    public EventInvitationTableCell(string cellId) : base(UITableViewCellStyle.Default, cellId)
	    {
	    }
	}
}
