using System;
using Foundation;
using UIKit;

namespace MyStarStable.iOS
{
    /// <summary>
    /// "Delegate" for the subject text field.
    /// </summary>
    class LimitTextFieldDelegate : UITextFieldDelegate
    {
        public int MaxTextLength { get; set; }

        /// <summary>
        /// Limits the message text to MaxTextLength characters.
        /// </summary>
        public override bool ShouldChangeCharacters(UITextField textField, NSRange range, string replacementString)
        {
            int limit = MaxTextLength;

            int newLength = (textField.Text.Length - (int)range.Length) + replacementString.Length;
            if (newLength <= limit)
                return true;

            int emptySpace = Math.Max(0, limit - (textField.Text.Length - (int)range.Length));
            string beforeCaret = textField.Text.Substring(0, (int)range.Location) + replacementString.Substring(0, emptySpace);
            string afterCaret = textField.Text.Substring((int)(range.Location + range.Length));

            textField.Text = beforeCaret + afterCaret;

            return false;
        }
    }

    /// <summary>
    /// "Delegate" for the message text view
    /// </summary>
    class LimitTextViewDelegate : UITextViewDelegate
    {
        public int MaxTextLength { get; set; }

        /// <summary>
        /// Limits the message text to MaxTextLength characters.
        /// </summary>
        public override bool ShouldChangeText(UITextView textView, NSRange range, string text)
        {
            int limit = MaxTextLength;

            int newLength = (textView.Text.Length - (int)range.Length) + text.Length;
            if (newLength <= limit)
                return true;

            int emptySpace = Math.Max(0, limit - (textView.Text.Length - (int)range.Length));
            string beforeCaret = textView.Text.Substring(0, (int)range.Location) + text.Substring(0, emptySpace);
            string afterCaret = textView.Text.Substring((int)(range.Location + range.Length));

            textView.Text = beforeCaret + afterCaret;
            textView.SelectedRange = new NSRange(beforeCaret.Length, 0);

            return false;
        }
    }
}