using System;
using System.Collections.Generic;
using System.Linq;

namespace MyStarStable.Common
{
    public sealed class CalendarHelper
    {
        #region Singleton
        private static Lazy<CalendarHelper> _lazyInstance = new Lazy<CalendarHelper>(() => new CalendarHelper());

        public static CalendarHelper Instance 
        { 
            get { return _lazyInstance.Value; } 
        }

        public static void Reset()
        {
            _lazyInstance = new Lazy<CalendarHelper>(() => new CalendarHelper());
        }
        #endregion

        public List<CalendarEvent> Events { get; set; }
        private HashSet<StableSession.YearMonth> _cachedMonths;

        public List<CalendarEvent> PendingEvents { get; set; }

        #region Events
        public event EventHandler EventsChanged;

        private void OnEventsChanged()
        {
            if (EventsChanged != null)
                EventsChanged(this, EventArgs.Empty);
        }

        public event EventHandler PendingEventsChanged;
        private void OnPendingEventsChanged()
        {
            if (PendingEventsChanged != null)
                PendingEventsChanged(this, EventArgs.Empty);
        }
        #endregion

        public CalendarHelper()
        {
            _cachedMonths = new HashSet<StableSession.YearMonth>(new StableSession.YearMonthComparer());
        }

        public void InvalidateCache(List<StableSession.YearMonth> months)
        {
            foreach (StableSession.YearMonth month in months)
            {
                _cachedMonths.Remove(month);
            }
        }

        public enum ModifyEventResult
        {
            Ok,
            Failed,
            Filtered
        }

        public delegate void ModifyEventResultDelegate(ModifyEventResult result, string reason, string filteredTitle, string filteredDescription);

        public delegate void AsyncCompletionDelegate(bool result, string reason);

        public void GetEventsAsync(int year, int month, AsyncCompletionDelegate resultDelegate)
        {
            List<StableSession.YearMonth> months = new List<StableSession.YearMonth>
            {
                new StableSession.YearMonth() { Year = year, Month = month }
            };
            GetEventsAsync(months, resultDelegate);
        }

        public void GetPendingEventsAsync(AsyncCompletionDelegate resultDelegate)
        {
            StableSession.Instance.GetPendingEventsAsync((events, reason) =>
            {
                PendingEvents = events;

                if (resultDelegate != null)
                    resultDelegate(events != null, reason);

                if (events != null)
                    OnPendingEventsChanged();
            });
        }

        public void GetEventsAsync(List<StableSession.YearMonth> months, AsyncCompletionDelegate resultDelegate)
        {
            List<StableSession.YearMonth> newMonths = months.Where(month => _cachedMonths.Contains(month) != true).ToList();
            if (newMonths.Count <= 0)
                return;


            StableSession.Instance.GetEventsAsync(newMonths, (events, reason) =>
            {
                if (events != null)
                {
                    if (Events != null)
                    {
                        foreach (StableSession.YearMonth month in newMonths)
                        {
                            DateTime startDate = new DateTime(month.Year, month.Month, 1);
                            DateTime endDate = new DateTime(month.Year, month.Month,
                                DateTime.DaysInMonth(month.Year, month.Month));
                            Events.RemoveAll(@event => @event.IsIn(startDate, endDate));
                        }
                    }

                    _cachedMonths.UnionWith(months);
                    Events = Events != null ? Events.Union(events, new EventComparer()).ToList() : events;
                    Events.Sort(new EventDateComparer());
                }

                if (resultDelegate != null)
                    resultDelegate(events != null, reason);

                if (events != null)
                    OnEventsChanged();
            });
        }

        public void AddEventAsync(CalendarEvent eventData, ModifyEventResultDelegate resultDelegate)
        {
            StableSession.Instance.AddEventAsync(eventData, (@modifiedEvent, months, reason, filteredTitle, filteredDescription) => 
            {
                if (@modifiedEvent != null)
                {
                    /* We don't need to do this right now. Maybe if we store data differently in the future.
                    InvalidateCache(months);
                    GetEventsAsync(months, resultDelegate);
                    */
                    Events.Add(@modifiedEvent);
                    Events.Sort(new EventDateComparer());
                }

                if (resultDelegate != null)
                {
                    ModifyEventResult result = ModifyEventResult.Failed;
                    if (@modifiedEvent != null)
                    {
                        result = ModifyEventResult.Ok;
                    }
                    else if (filteredTitle != null || filteredDescription != null)
                    {
                        result = ModifyEventResult.Filtered;
                    }
                    resultDelegate(result, reason, filteredTitle, filteredDescription);
                }

                if (@modifiedEvent != null)
                    OnEventsChanged();
            });
        }

        public void EditEventAsync(CalendarEvent eventData, ModifyEventResultDelegate resultDelegate)
        {
            StableSession.Instance.EditEventAsync(eventData, (@modifiedEvent, months, reason, filteredTitle, filteredDescription) =>
            {
                if (@modifiedEvent != null)
                {
                    Events.RemoveAll(@event => @event.EventId == @modifiedEvent.EventId);
                    Events.Add(@modifiedEvent);
                    Events.Sort(new EventDateComparer());

                    /* We don't need to do this right now. Maybe if we store data differently in the future.
                    if (months != null)
                    {
                        InvalidateCache(months);
                        GetEventsAsync(months, resultDelegate);
                        return;
                    }*/
                }
                if (resultDelegate != null)
                {
                    ModifyEventResult result = ModifyEventResult.Failed;
                    if (@modifiedEvent != null)
                    {
                        result = ModifyEventResult.Ok;
                    }
                    else if (filteredTitle != null || filteredDescription != null)
                    {
                        result = ModifyEventResult.Filtered;
                    }

                    resultDelegate(result, reason, filteredTitle, filteredDescription);
                }

                if (@modifiedEvent != null)
                    OnEventsChanged();
            });
        }

        public void ReplyInvitationAsync(UInt64 eventId, bool accept, AsyncCompletionDelegate resultDelegate)
        {
            StableSession.Instance.ReplyInvitationAsync(eventId, accept, (eventData, reason) => 
            {
                if (eventData != null)
                {
                    PendingEvents.RemoveAll(@event => @event.EventId == eventId);
                    if (accept)
                    {
                        Events.Add(eventData);
                        Events.Sort(new EventDateComparer());
                    }
                }

                if (resultDelegate != null)
                {
                    resultDelegate(eventData != null, reason);
                }

                if (eventData != null)
                {
                    OnEventsChanged();
                    OnPendingEventsChanged();
                }

            });
        }

        public void DeleteEventAsync(CalendarEvent eventData, AsyncCompletionDelegate resultDelegate)
        {
            StableSession.Instance.DeleteEventAsync(eventData.EventId.Value, (result, reason) =>
            {
                if (result)
                {
                    PendingEvents.RemoveAll(@event => @event.EventId == eventData.EventId);
                    Events.RemoveAll(@event => @event.EventId == eventData.EventId);
                }

                if (resultDelegate != null)
                {
                    resultDelegate(result, reason);
                }

                if (result)
                {
                    OnEventsChanged();
                    OnPendingEventsChanged();
                }

            });
        }

        public bool DayHasEvent(DateTime time)
        {
            if (Events != null)
            {
                return (Events.Find(@event => @event.IsAt(time)) != null);
            } 
            else 
            {
                return false;
            }
        }

        public List<CalendarEvent> GetEventsForDate(DateTime date)
        {
            List<CalendarEvent> eventData = null;
            if (Events != null)
            {
                eventData = Events.Where(@event => @event.IsAt(date)).ToList();
            }
            return eventData;
        }
    }
}
