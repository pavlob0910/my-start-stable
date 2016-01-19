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
using Foundation;
using UIKit;
using CoreGraphics;
using System.Globalization;

namespace Factorymind.Components
{
    [Register("FMCalendar")]
    public class FMCalendar : UIView
	{
        public static String BasePath = "FMCalendar/";

		/// <summary>
		/// Fired when new date selected.
		/// </summary>
		public Action<DateTime> DateSelected;

		/// <summary>
		/// Fired when Selected month changed
		/// </summary>
		public Action<DateTime, DateTime> MonthChanged;

		/// <summary>
		/// Mark with a dot dates that fulfill the predicate
		/// </summary>
		public Func<DateTime, bool> IsDayMarkedDelegate;

		/// <summary>
		/// Turn gray dates that fulfill the predicate
		/// </summary>
		public Func<DateTime, bool> IsDateAvailable;

		public DateTime CurrentMonthYear;
		protected DateTime CurrentDate { get; set; }

        public UIScrollView ScrollView { get; private set; }
		private bool _calendarIsLoaded;

		private MonthGridView _monthGridView;
		private UIButton _leftButton, _rightButton;

		// User Customizations

		/// <summary>
		/// If true, Sunday will be showed as the first day of the week, otherwise the first one will be Monday
		/// </summary>
		/// <value><c>true</c> if sunday first; otherwise, <c>false</c>.</value>
		public Boolean SundayFirst { get; set; }

		/// <summary>
		/// Format string used to display the month's name
		/// </summary>
		/// <value>The month format string.</value>
		public String MonthFormatString { get; set; }

		/// <summary>
		/// Specify the color for the selected date
		/// </summary>
		/// <value>The color of the selection.</value>
		public UIColor SelectionColor { get; set; }

		/// <summary>
		/// Gets or sets the left arrow image (32x32 px).
		/// </summary>
		/// <value>The left arrow.</value>
		public UIImage LeftArrow { get; set; }

		/// <summary>
		/// Gets or sets the right arrow image (32x32 px).
		/// </summary>
		/// <value>The right arrow.</value>
		public UIImage RightArrow { get; set; }

		/// <summary>
		/// Gets or sets the top bar image (320x44 px).
		/// </summary>
		/// <value>The top bar.</value>
		public UIImage TopBar { get; set; }

        private void Initialize()
        {
            CurrentDate = DateTime.Now.Date;
            CurrentMonthYear = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
            BackgroundColor = UIColor.White;

            // Defaults

            SundayFirst = false;

            MonthFormatString = "MMMM yyyy";

            SelectionColor = UIColor.Red;

            LeftArrow = UIImage.FromFile(BasePath + "leftArrow.png");
            RightArrow = UIImage.FromFile(BasePath + "rightArrow.png");
            TopBar = UIImage.FromFile(BasePath + "topbar.png");

            /*
            UISwipeGestureRecognizer swipeRightRecognizer = new UISwipeGestureRecognizer(SwipeAction);
            swipeRightRecognizer.Direction = UISwipeGestureRecognizerDirection.Right;
            AddGestureRecognizer(swipeRightRecognizer);
            
            UISwipeGestureRecognizer swipeLeftRecognizer = new UISwipeGestureRecognizer(SwipeAction);
            swipeLeftRecognizer.Direction = UISwipeGestureRecognizerDirection.Left;
            AddGestureRecognizer(swipeLeftRecognizer);
            */

            UISwipeGestureRecognizer swipeDownRecognizer = new UISwipeGestureRecognizer(SwipeAction);
            swipeDownRecognizer.Direction = UISwipeGestureRecognizerDirection.Down;
            AddGestureRecognizer(swipeDownRecognizer);

            UISwipeGestureRecognizer swipeUpRecognizer = new UISwipeGestureRecognizer(SwipeAction);
            swipeUpRecognizer.Direction = UISwipeGestureRecognizerDirection.Up;
            AddGestureRecognizer(swipeUpRecognizer);
        }

        private void SwipeAction(UISwipeGestureRecognizer uiSwipeGestureRecognizer)
        {
            if (uiSwipeGestureRecognizer.Direction == UISwipeGestureRecognizerDirection.Down ||
                uiSwipeGestureRecognizer.Direction == UISwipeGestureRecognizerDirection.Right)
            {
                MoveCalendarMonths(false, true);
            }
            else if (uiSwipeGestureRecognizer.Direction == UISwipeGestureRecognizerDirection.Up ||
                     uiSwipeGestureRecognizer.Direction == UISwipeGestureRecognizerDirection.Left)
            {
                MoveCalendarMonths(true, true);
            }
        }

        public FMCalendar(IntPtr handle) : base(handle)
        {
        }

        public override void AwakeFromNib()
        {
            Initialize();
        }

		public FMCalendar() : base(new CGRect(0, 0, 320, 400))
		{
            Initialize();
		}

		public override void SetNeedsDisplay ()
		{
			base.SetNeedsDisplay();

			if (_monthGridView!=null)
				_monthGridView.Update();
		}

		public override void LayoutSubviews ()
		{
            base.LayoutSubviews();

		    if (!_calendarIsLoaded)
		    {

		        ScrollView = new UIScrollView(new CGRect(0, 44, Frame.Width, Frame.Height - 44))
		        {
		            ContentSize = new CGSize(Frame.Width, Frame.Height - 44),
		            //ScrollEnabled = false,
		            //Frame = new RectangleF(0, 44, Frame.Width, Frame.Height - 44),
		            BackgroundColor = UIColor.White,
		        };
		        LoadButtons();

                _monthGridView = CreateNewGrid(CurrentMonthYear);

		        AddSubview(ScrollView);
		        ScrollView.AddSubview(_monthGridView);

		        _calendarIsLoaded = true;
		    }
            _leftButton.Frame = new CGRect(10, 0, 32, 32);
            _rightButton.Frame = new CGRect(Frame.Right - 56, 0, 32, 32);

            _monthGridView.RebuildGrid();

            ScrollView.Frame = new CGRect(0, 44, Frame.Width, _monthGridView.Frame.Height);
		    ScrollView.ContentSize = _monthGridView.Frame.Size;

            InvalidateIntrinsicContentSize();

            SetNeedsDisplay();
		}

        public override CGSize IntrinsicContentSize
        {
            get { return new CGSize(NoIntrinsicMetric, _monthGridView != null ? _monthGridView.Frame.Height + 44 : 44); }
        }

        public void DeselectDate(){
			if (_monthGridView!=null)
				_monthGridView.DeselectDayView();
		}

		private void LoadButtons()
		{
			_leftButton = UIButton.FromType(UIButtonType.Custom);
			_leftButton.TouchUpInside += HandlePreviousMonthTouch;
            _leftButton.ImageEdgeInsets = new UIEdgeInsets(5.0f, 9.5f, 5.0f, 9.5f);
            _leftButton.ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
			_leftButton.SetImage(LeftArrow, UIControlState.Normal);
			AddSubview(_leftButton);

			_rightButton = UIButton.FromType(UIButtonType.Custom);
			_rightButton.TouchUpInside += HandleNextMonthTouch;
            _rightButton.ImageEdgeInsets = new UIEdgeInsets(5.0f, 9.5f, 5.0f, 9.5f);
            _rightButton.ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            _rightButton.SetImage(RightArrow, UIControlState.Normal);
			AddSubview(_rightButton);
		}

		private void HandlePreviousMonthTouch(object sender, EventArgs e)
		{
			MoveCalendarMonths(false, true);
		}
		private void HandleNextMonthTouch(object sender, EventArgs e)
		{
			MoveCalendarMonths(true, true);
		}

		public void MoveCalendarMonths(bool upwards, bool animated)
		{
		    var oldMonthYear = CurrentMonthYear;
			CurrentMonthYear = CurrentMonthYear.AddMonths(upwards? 1 : -1);

			// Dispatch event
			if (MonthChanged != null)
                MonthChanged(oldMonthYear, CurrentMonthYear);

		    nfloat yOffset = (upwards ? _monthGridView.Frame.Height : -_monthGridView.Frame.Height);
            
			var gridToMove = CreateNewGrid(CurrentMonthYear);
            gridToMove.BuildGrid();
		    gridToMove.Frame = new CGRect(gridToMove.Frame.X, gridToMove.Frame.Y + yOffset, gridToMove.Frame.Width, gridToMove.Frame.Height);

			ScrollView.AddSubview(gridToMove);

            Animate(0.4f, 0.0f, UIViewAnimationOptions.CurveEaseInOut, () =>
            {
                _monthGridView.Center = new CGPoint(_monthGridView.Center.X, _monthGridView.Center.Y - yOffset);
                _monthGridView.Alpha = 0;

                gridToMove.Center = new CGPoint(gridToMove.Center.X, gridToMove.Center.Y - yOffset);
            }, () =>
            {
                _monthGridView.RemoveFromSuperview();
                _monthGridView = gridToMove;
                SetNeedsDisplay();
            });
		}

		private MonthGridView CreateNewGrid(DateTime date){
			var grid = new MonthGridView(this, date);
			grid.CurrentDate = CurrentDate;
			return grid;
		}

		public override void Draw(CGRect rect)
		{
			TopBar.Draw(new CGPoint(0,0));
			DrawDayLabels(rect);
			DrawMonthLabel(rect);
		}

		private void DrawMonthLabel(CGRect rect)
		{
			var r = new CGRect(new CGPoint(0, 5), new CGSize {Width = Frame.Width, Height = 42});
			UIColor.Black.SetColor ();
            NSString nativeText = new NSString(CurrentMonthYear.ToString(MonthFormatString, new CultureInfo(NSLocale.CurrentLocale.LanguageCode)));
			nativeText.DrawString(r, UIFont.SystemFontOfSize(20), UILineBreakMode.WordWrap, UITextAlignment.Center);
		}

		private void DrawDayLabels(CGRect rect)
		{
			var font = UIFont.SystemFontOfSize(10);
			UIColor.Black.SetColor ();
			var context = UIGraphics.GetCurrentContext();
			context.SaveState();

            var cultureInfo = new CultureInfo(NSLocale.CurrentLocale.LanguageCode);

			cultureInfo.DateTimeFormat.FirstDayOfWeek = DayOfWeek.Monday;
			var dayNames = cultureInfo.DateTimeFormat.AbbreviatedDayNames;

			if (!SundayFirst) 
			{
				// Shift Sunday to the end of the week
				var firstDay = dayNames [0];
				for (int count = 0; count < dayNames.Length - 1; count++)
					dayNames [count] = dayNames [count + 1];
				dayNames [dayNames.Length - 1] = firstDay;
			}

            CGRect gridFrame = ConvertRectFromView(_monthGridView.Frame, ScrollView);
            for (int colNum = 0; colNum < _monthGridView.Columns; ++colNum)
            {
                NSString nativeString = new NSString(dayNames[colNum % dayNames.Length]);
                nativeString.DrawString(new CGRect(gridFrame.X + colNum * 45, 44 - 12, 45, 10), font, UILineBreakMode.WordWrap, UITextAlignment.Center);
			}
			context.RestoreState();
		}

        public void SelectToday()
        {
            _monthGridView.SelectToday();
        }
	}
}