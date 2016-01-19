using CoreGraphics;
using UIKit;
using System;

namespace MyStarStable.iOS
{
    public static class ScrollExtensions
    {
        public static void CenterView(this UIScrollView scrollView, UIView viewToCenter, CGRect keyboardFrame, bool animated = false) {
            var scrollFrame = scrollView.Frame;

            var adjustedFrame = UIApplication.SharedApplication.KeyWindow.ConvertRectFromView(scrollFrame, scrollView.Superview);

            var intersect = CGRect.Intersect(adjustedFrame, keyboardFrame);

            bool ignoreOrientation = UIDevice.CurrentDevice.CheckSystemVersion(8, 0);

            scrollView.CenterView(viewToCenter, (IsLandscape() && !ignoreOrientation) ? intersect.Width : intersect.Height, animated:animated);
        }

        public static void CenterView(this UIScrollView scrollView, UIView viewToCenter, nfloat keyboardHeight, bool adjustContentInsets = true, bool animated = false)
        {
            if (adjustContentInsets)
            {
                var contentInsets = new UIEdgeInsets(0.0f, 0.0f, keyboardHeight, 0.0f);
                scrollView.ContentInset = contentInsets;
                scrollView.ScrollIndicatorInsets = contentInsets;
            }

            // Position of the active field relative isnside the scroll view
            CGRect relativeFrame = viewToCenter.Superview.ConvertRectToView(viewToCenter.Frame, scrollView);

            var spaceAboveKeyboard = scrollView.Frame.Height - keyboardHeight;

            // Move the active field to the center of the available space
            var offset = relativeFrame.Y - (spaceAboveKeyboard - viewToCenter.Frame.Height) / 2;
            if (scrollView.ContentOffset.Y < offset) {
                scrollView.SetContentOffset(new CGPoint(0, offset), animated);
            }
        }

        public static void RestoreScrollPosition(this UIScrollView scrollView)
        {
            scrollView.ContentInset = UIEdgeInsets.Zero;
            scrollView.ScrollIndicatorInsets = UIEdgeInsets.Zero;
        }

        public static bool IsLandscape() {
            var orientation = UIApplication.SharedApplication.StatusBarOrientation;
            bool landscape = orientation == UIInterfaceOrientation.LandscapeLeft 
                        || orientation == UIInterfaceOrientation.LandscapeRight;
            return landscape;
        }
    }
}