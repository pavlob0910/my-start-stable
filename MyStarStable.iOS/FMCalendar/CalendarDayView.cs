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

namespace Factorymind.Components
{
	class CalendarDayView : UIView
	{
		string _text;
		public DateTime Date {get;set;}
		bool _active, _today, _selected, _marked, _available;
		public bool Available {get {return _available; } set {_available = value; SetNeedsDisplay(); }}
		public string Text {get { return _text; } set { _text = value; SetNeedsDisplay(); } }
		public bool Active {get { return _active; } set { _active = value; SetNeedsDisplay();  } }
		public bool Today {get { return _today; } set { _today = value; SetNeedsDisplay(); } }
		public bool Selected {get { return _selected; } set { _selected = value; SetNeedsDisplay(); } }
		public bool Marked {get { return _marked; } set { _marked = value; SetNeedsDisplay(); }  }

		public UIColor SelectionColor { get; set; }

		public override void Draw(CGRect rect)
		{
			if (SelectionColor == null)
				SelectionColor = UIColor.Red;

			UIImage img = UIImage.FromFile(FMCalendar.BasePath + "datecell.png");
			UIColor color = UIColor.Black;

			if (!Active || !Available)
			{
				color = UIColor.FromRGBA(0.576f, 0.608f, 0.647f, 1f);
				if(Selected)
					color = SelectionColor;
			} else if (Today && Selected)
			{
				color = UIColor.White;
                img = UIImage.FromFile(FMCalendar.BasePath + "today.png");
			} else if (Today)
			{
				color = UIColor.White;
                img = UIImage.FromFile(FMCalendar.BasePath + "today.png");
			} else if (Selected)
			{
				color = SelectionColor;
			}

			img.Draw(new CGPoint(0, 0));
			color.SetColor();

            NSString nativeText = new NSString(Text);
		    nativeText.DrawString(CGRect.Inflate(Bounds, 4, -8),
		        UIFont.SystemFontOfSize(20),
		        UILineBreakMode.WordWrap, UITextAlignment.Center);

			if (Marked)
			{
				var context = UIGraphics.GetCurrentContext();
				if (Selected && !Today)
					SelectionColor.SetColor ();
				else if (Today)
					UIColor.White.SetColor ();
				else if (!Active || !Available)
					UIColor.LightGray.SetColor ();
				else
					UIColor.Black.SetColor ();

				context.SetLineWidth(0);
				context.AddEllipseInRect(new CGRect(Frame.Size.Width/2 - 2, 45-10, 4, 4));
				context.FillPath();

			}
		}
	}
}

