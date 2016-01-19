using CoreGraphics;
using Foundation;
using UIKit;

namespace MyStarStable.iOS
{
    public class ControllerKeyboardHandler
    {
        protected UIViewController Controller;

        public ControllerKeyboardHandler(UIViewController controller)
        {
            Controller = controller;
        }

        /// <summary>
        /// Call this method from constructor, ViewDidLoad or ViewWillAppear to enable keyboard handling.
        /// </summary>
        public void InitKeyboardHandling()
        {
            RegisterForKeyboardNotifications();
        }

        /// <summary>
        /// Call it to force dismiss keyboard when background is tapped
        /// </summary>
        public void DismissKeyboardOnBackgroundTap()
        {
            // Add gesture recognizer to hide keyboard
            var tap = new UITapGestureRecognizer { CancelsTouchesInView = false };
            tap.AddTarget(() => Controller.View.EndEditing(true));
            tap.ShouldReceiveTouch = (recognizer, touch) =>
                !(touch.View is UIControl || touch.View.FindSuperviewOfType(Controller.View, typeof(UITableViewCell)) != null);
            Controller.View.AddGestureRecognizer(tap);
        }


        /// <summary>
        /// Set this field to any view inside the textfield to center this view instead of the current responder
        /// </summary>
        protected UIView ViewToCenterOnKeyboardShown;
        protected UIScrollView ScrollToCenterOnKeyboardShown;

        NSObject _keyboardShowObserver;
        NSObject _keyboardHideObserver;
        protected void RegisterForKeyboardNotifications()
        {
            if (_keyboardShowObserver == null)
                _keyboardShowObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, OnKeyboardNotification);
            if (_keyboardHideObserver == null)
                _keyboardHideObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, OnKeyboardNotification);
        }

        protected void UnregisterForKeyboardNotifications()
        {
            if (_keyboardShowObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_keyboardShowObserver);
                _keyboardShowObserver.Dispose();
                _keyboardShowObserver = null;
            }

            if (_keyboardHideObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(_keyboardHideObserver);
                _keyboardHideObserver.Dispose();
                _keyboardHideObserver = null;
            }
        }

        /// <summary>
        /// Gets the UIView that represents the "active" user input control (e.g. textfield, or button under a text field)
        /// </summary>
        /// <returns>
        /// A <see cref="UIView"/>
        /// </returns>
        protected UIView KeyboardGetActiveView()
        {
            return Controller.View.FindFirstResponder();
        }

        private void OnKeyboardNotification (NSNotification notification)
        {
            if (!Controller.IsViewLoaded) return;

            //Check if the keyboard is becoming visible
            var visible = notification.Name == UIKeyboard.WillShowNotification;

            //Start an animation, using values from the keyboard
            UIView.BeginAnimations ("AnimateForKeyboard");
            UIView.SetAnimationBeginsFromCurrentState (true);
            UIView.SetAnimationDuration (UIKeyboard.AnimationDurationFromNotification (notification));
            UIView.SetAnimationCurve ((UIViewAnimationCurve)UIKeyboard.AnimationCurveFromNotification (notification));

            //Pass the notification, calculating keyboard height, etc.
            var keyboardFrame = visible
                                ? UIKeyboard.FrameEndFromNotification(notification)
                                : UIKeyboard.FrameBeginFromNotification(notification);

            OnKeyboardChanged (visible, keyboardFrame);

            //Commit the animation
            UIView.CommitAnimations ();
        }

        /// <summary>
        /// Override this method to apply custom logic when the keyboard is shown/hidden
        /// </summary>
        /// <param name='visible'>
        /// If the keyboard is visible
        /// </param>
        /// <param name='keyboardFrame'>
        /// Frame of the keyboard
        /// </param>
        protected void OnKeyboardChanged (bool visible, CGRect keyboardFrame)
        {
            var activeView = ViewToCenterOnKeyboardShown ?? KeyboardGetActiveView();
            if (activeView == null)
                return;

            var scrollView = ScrollToCenterOnKeyboardShown ??
                             activeView.FindTopSuperviewOfType(Controller.View, typeof(UIScrollView)) as UIScrollView;
            if (scrollView == null)
                return;

            if (!visible)
                scrollView.RestoreScrollPosition();
            else
                scrollView.CenterView(activeView, keyboardFrame);
        }
    }
}

