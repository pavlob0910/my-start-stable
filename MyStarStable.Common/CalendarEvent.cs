using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MyStarStable.Common
{
    public class EventComparer : IEqualityComparer<CalendarEvent>
    {
        public bool Equals(CalendarEvent x, CalendarEvent y)
        {
            if (Object.ReferenceEquals(x, y)) return true;
            return x != null && y != null && (x.EventId == y.EventId);
        }

        public int GetHashCode(CalendarEvent obj)
        {
            return obj.EventId.GetHashCode();
        }
    }

    public class EventDateComparer : IComparer<CalendarEvent>
    {
        public int Compare(CalendarEvent x, CalendarEvent y)
        {
            if (x == null)
            {
                if (y == null)
                    return 0;

                return -1;
            }

            if (y == null)
                return 1;

            return x.StartDate.CompareTo(y.StartDate);
        }
    }

    public class CalendarEvent
    {
        public CalendarEvent()
        {
            EventId = null;
            Type = null;
            Title = "";
            Description = "";
            ClubId = null;
            ServerId = null;
            Attendees = new List<CalendarEventAttendee>();
        }

        public enum EventType : int
        {
            Personal = 0,
            Club = 1,
            Server = 2
        };

        [JsonProperty("event_id", NullValueHandling = NullValueHandling.Ignore)]
        public UInt64? EventId;

        [JsonProperty("event_type", NullValueHandling = NullValueHandling.Ignore)]
        public EventType? Type;

        [JsonProperty("start_date")]
        public DateTime StartDate;

        [JsonProperty("end_date")]
        public DateTime EndDate;

        [JsonProperty("title")]
        public string Title;

        [JsonProperty("description")]
        public string Description;

        [JsonProperty("club_id", NullValueHandling = NullValueHandling.Ignore)]
        public int? ClubId;

        [JsonProperty("server_id", NullValueHandling = NullValueHandling.Ignore)]
        public int? ServerId;

        [JsonProperty("attendees")]
        public List<CalendarEventAttendee> Attendees;


        public bool IsAt(DateTime time)
        {
            DateTime localStartDate = StartDate.ToLocalTime();
            DateTime localEndDate = EndDate.ToLocalTime();

            if (time.Date >= localStartDate.Date && time.Date <= localEndDate.Date)
                return true;

            return false;
        }

        public bool IsIn(DateTime startDate, DateTime endDate)
        {
            DateTime localStartDate = StartDate.ToLocalTime();
            DateTime localEndDate = EndDate.ToLocalTime();

            if (startDate.Date >= localStartDate.Date && startDate.Date <= localEndDate.Date)
                return true;

            if (endDate.Date >= localStartDate.Date && endDate.Date <= localEndDate.Date)
                return true;

            return false;
        }
    }
}
