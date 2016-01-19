using System;
using Newtonsoft.Json;

namespace MyStarStable.Common
{
    public class CalendarEventAttendee
    {
        public enum AttendeeStatus : int
        {
            Invited = 0,
            Owner = 1,
            Accepted = 2,
            Rejected = 3,
        };

        [JsonProperty("character_id")]
        public UInt64 CharacterId;

        [JsonProperty("first_name", NullValueHandling = NullValueHandling.Ignore)]
        public string FirstName;

        [JsonProperty("last_name", NullValueHandling = NullValueHandling.Ignore)]
        public string LastName;

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public AttendeeStatus? Status;
    }
}
