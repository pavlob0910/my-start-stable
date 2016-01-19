//	New implementations, refactoring and restyling - FactoryMind || http://factorymind.com 
//  Converted to MonoTouch on 1/22/09 - Eduardo Scoz || http://escoz.com
//  Originally reated by Devin Ross on 7/28/09  - tapku.com || http://github.com/devinross/tapkulibrary
//
/*
 
 Permission is hereby granted, free of charge, to any person
 obtaining a copy of this software and associated documentation
 files (the "Software"), to deal in the Software without
 restriction, including without limitation the rights to use,
 copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the
 Software is furnished to do so, subject to the following
 conditions:
 
 The above copyright notice and this permission notice shall be
 included in all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 OTHER DEALINGS IN THE SOFTWARE.
 
 */

using System;
using System.Linq;
using UIKit;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;

namespace Factorymind.Components
{
	class MonthGridView : UIView
	{
		private FMCalendar _calendarMonthView;

		public DateTime CurrentDate {get;set;}
		private DateTime _currentMonth;
		protected readonly IList<CalendarDayView> DayTiles = new List<CalendarDayView>();
		
        public int Lines { get; protected set; }
        public int Columns { get; protected set; }

		protected CalendarDayView SelectedDayView {get;set;}
		public int weekdayOfFirst;
		public IList<DateTime> Marks { get; set; }

		public MonthGridView(FMCalendar calendarMonthView, DateTime month)
		{
			_calendarMonthView = calendarMonthView;
			_currentMonth = month.Date;
		}

		public void Update(){
			foreach (var v in DayTiles)
				updateDayView(v);

			SetNeedsDisplay();
		}

		public void updateDayView(CalendarDayView dayView)
        {
			dayView.Marked = _calendarMonthView.IsDayMarkedDelegate != null && _calendarMonthView.IsDayMarkedDelegate(dayView.Date);
			dayView.Available = _calendarMonthView.IsDateAvailable == null || _calendarMonthView.IsDateAvailable(dayView.Date);
		}

		private int WeekDayIndex(DayOfWeek day)
		{
			var index = (int)day;

			if (!_calendarMonthView.SundayFirst) {
				if (day == DayOfWeek.Sunday)
					index = (int)DayOfWeek.Saturday;
				else
					index--;
			}

			return index;
		}

	    public void RebuildGrid()
	    {
	        foreach (UIView view in Subviews)
	        {
	            view.RemoveFromSuperview();
	        }
            DayTiles.Clear();

	        BuildGrid();

	    }


		public void BuildGrid()
		{
            //BackgroundColor = UIColor.Red;

            nfloat parentFrameWidth = _calendarMonthView.Frame.Width;
            int numDaysWide = (int)(parentFrameWidth/ 45.0f);
		    nfloat daysWidth = numDaysWide*45.0f;

			DateTime previousMonth = _currentMonth.AddMonths(-1);
			DateTime nextMonth = _currentMonth.AddMonths(1);
			var daysInPreviousMonth = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
			var daysInMonth = DateTime.DaysInMonth(_currentMonth.Year, _currentMonth.Month);
			weekdayOfFirst = WeekDayIndex(_currentMonth.DayOfWeek);
			var lead = daysInPreviousMonth - (weekdayOfFirst - 1);

			// build last month's days
			for (int i = 1; i <= weekdayOfFirst; i++)
			{
                var viewDay = new DateTime(previousMonth.Year, previousMonth.Month, lead);
				var dayView = new CalendarDayView() { SelectionColor = _calendarMonthView.SelectionColor };
				dayView.Frame = new CGRect((i - 1) * 45, 0, 45, 40);
				dayView.Date = viewDay;
				dayView.Text = lead.ToString();
                dayView.Tag = viewDay.Day;

				AddSubview(dayView);
				DayTiles.Add(dayView);
				lead++;
			}

			var position = weekdayOfFirst+1;
			var line = 0;

			// current month
			for (int i = 1; i <= daysInMonth; i++)
			{
				var viewDay = new DateTime(_currentMonth.Year, _currentMonth.Month, i);
				var dayView = new CalendarDayView
				{
					Frame = new CGRect((position - 1) * 45, line * 40, 45, 40),
					Today = (CurrentDate.Date==viewDay.Date),
					Text = i.ToString(),

					Active = true,
					Tag = i,
					SelectionColor = _calendarMonthView.SelectionColor,
				};
				dayView.Date = viewDay;
				updateDayView(dayView);

				if (dayView.Selected)
					SelectedDayView = dayView;

				AddSubview(dayView);
				DayTiles.Add(dayView);

				position++;
                if (position > numDaysWide)
				{
					position = 1;
					line++;
				}
			}

			//next month
		    if (position != 1)
		    {
		        int dayCounter = 1;
		        for (int i = position; i < numDaysWide; i++)
		        {
                    var viewDay = new DateTime(nextMonth.Year, nextMonth.Month, dayCounter);
		            var dayView = new CalendarDayView
		            {
		                Frame = new CGRect((i - 1)*45, line*40, 45, 40),
		                Text = dayCounter.ToString(),
                        Tag = viewDay.Day,
		                SelectionColor = _calendarMonthView.SelectionColor,
		            };
		            dayView.Date = viewDay;
		            updateDayView(dayView);

		            AddSubview(dayView);
		            DayTiles.Add(dayView);
		            dayCounter++;
		        }
		    }
		    else
		    {
		        line--;
		    }

		    nfloat xPos = Math.Max(0, (float)(parentFrameWidth - daysWidth)/2.0f);
            Frame = new CGRect(new CGPoint(xPos, 0), new CGSize(daysWidth, (line + 1) * 40));

		    Lines = line + 1;
		    Columns = numDaysWide;

			if (SelectedDayView!=null)
				BringSubviewToFront(SelectedDayView);
		}

		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			base.TouchesBegan (touches, evt);

            bool hitDate = SelectDayView(((UITouch)touches.AnyObject).View);
            
            if (hitDate && _calendarMonthView.DateSelected != null)
				_calendarMonthView.DateSelected(new DateTime(_currentMonth.Year, _currentMonth.Month, (int)SelectedDayView.Tag));
		}

		private bool SelectDayView(UIView view)
		{
		    CalendarDayView newDayView = view as CalendarDayView;
		    if (newDayView == null)
		        return false;

		    if (newDayView.Active != true)
		    {
		        _calendarMonthView.MoveCalendarMonths(newDayView.Tag <= 15, true);
		        return false;
		    }

		    if (SelectedDayView != null)
                SelectedDayView.Selected = false;
            
            BringSubviewToFront(newDayView);
            newDayView.Selected = true;
		    SelectedDayView = newDayView;

            SetNeedsDisplay();
		    return true;
		}

		public void DeselectDayView(){
			if (SelectedDayView==null) return;
			SelectedDayView.Selected= false;
			SelectedDayView = null;
			SetNeedsDisplay();
		}

	    public void SelectToday()
	    {
	        SelectDayView(DayTiles.First(view => view.Today));

            if (_calendarMonthView.DateSelected != null)
                _calendarMonthView.DateSelected(new DateTime(_currentMonth.Year, _currentMonth.Month, (int)SelectedDayView.Tag));
	    }
	}
}