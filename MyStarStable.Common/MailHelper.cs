using System;
using System.Collections.Generic;
using System.Linq;

namespace MyStarStable.Common
{
    public sealed class MailHelper
    {
        #region Singleton
        private static Lazy<MailHelper> _lazyInstance = new Lazy<MailHelper>(() => new MailHelper());

        public static MailHelper Instance
        {
            get { return _lazyInstance.Value; }
        }

        public static void Reset()
        {
            _lazyInstance = new Lazy<MailHelper>(() => new MailHelper());
        }
        #endregion

        public bool LoadingMail { get; private set; }

        private List<CharacterMailMessage> _messages;
        public List<CharacterMailMessage> Messages
        {
            get { return _messages; }
            set
            {
                _messages = value;
                if (_messages != null)
                    _messages.Sort(Comparer<CharacterMailMessage>.Create((msg1, msg2) => msg2.SentDate.CompareTo(msg1.SentDate)));
            }
        }
        public int UnreadCount
        {
            get
            {
                if (Messages == null)
                    return 0;

                return Messages.Count(message => message.ReadDate == null || (message.ReadDate != null && message.ReadDate == DateTime.MinValue));
            }
            private set { }
        }

        #region Events
        public class UnreadCountChangedEventArgs : EventArgs
        {
            public UnreadCountChangedEventArgs(int unreadCount)
            {
                UnreadCount = unreadCount;
            }

            public int UnreadCount { get; private set; }
        }

        public event EventHandler<UnreadCountChangedEventArgs> UnreadCountChanged;

        private void OnUnreadCountChanged()
        {
            if (UnreadCountChanged != null)
            {
                UnreadCountChangedEventArgs e = new UnreadCountChangedEventArgs(UnreadCount);
                UnreadCountChanged(this, e);
            }
        }

        public event EventHandler MessagesChanged;

        private void OnMessagesChanged()
        {
            if (MessagesChanged != null)
                MessagesChanged(this, EventArgs.Empty);
        }
        #endregion

        public MailHelper()
        {
        }

        public delegate void AsyncCompletionDelegate(bool result, string reason);

        public void GetMailAsync(AsyncCompletionDelegate resultDelegate)
        {
            LoadingMail = true;

            StableSession.Instance.GetMailAsync((messages, reason) =>
            {
                Messages = messages;
                LoadingMail = false;

                if (resultDelegate != null)
                    resultDelegate(Messages != null, reason);

                OnMessagesChanged();
                OnUnreadCountChanged();
            });
        }

        public void MarkMailReadAsync(CharacterMailMessage message, AsyncCompletionDelegate resultDelegate)
        {
            StableSession.Instance.MarkMailReadAsync(message.MailId, (bool result, string reason) =>
            {
                if (result)
                {
                    message.ReadDate = DateTime.UtcNow;
                }

                if (resultDelegate != null)
                    resultDelegate(result, reason);

                if (!result) 
                    return;

                OnMessagesChanged();
                OnUnreadCountChanged();
            });
        }

        public void DeleteMailAsync(CharacterMailMessage message, AsyncCompletionDelegate resultDelegate)
        {
            StableSession.Instance.DeleteMailAsync(message.MailId, (bool result, string reason) =>
            {
                if (result)
                {
                    Messages.Remove(message);
                }

                if (resultDelegate != null)
                    resultDelegate(result, reason);

                if (!result)
                    return;

                OnMessagesChanged();
                OnUnreadCountChanged();
            });
        }
    }
}
