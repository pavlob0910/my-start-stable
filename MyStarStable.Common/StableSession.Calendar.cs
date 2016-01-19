using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyStarStable.Common
{
    public sealed partial class StableSession
    {
        public delegate void GetEventsResultDelegate(List<CalendarEvent> events, string reason);
        public delegate void ModifyEventResultDelegate(CalendarEvent modifiedEvent, List<YearMonth> months, string reason, string filteredTitle, string filteredDescription);
        public delegate void ReplyInvitationDelegate(CalendarEvent eventData, string reason);
        public delegate void DeleteEventResultDelegate(bool result, string reason);

        public class YearMonth
        {
            [JsonProperty("year")]
            public int Year;
            [JsonProperty("month")]
            public int Month;
        }

        public class YearMonthComparer : IEqualityComparer<YearMonth>
        {
            public bool Equals(YearMonth x, YearMonth y)
            {
                if (Object.ReferenceEquals(x, y)) return true;
                return x != null && y != null && (x.Year == y.Year) && (x.Month == y.Month);
            }

            public int GetHashCode(YearMonth obj)
            {
                return obj.Year.GetHashCode() ^ obj.Month.GetHashCode();
            }
        }

        public void GetPendingEventsAsync(GetEventsResultDelegate resultDelegate)
        {
            if (IsActive == false)
                return;

            ApiRequest apiReq = new ApiRequest(_settings.AuthorizationTokenId, _settings.AuthorizationToken)
            {
                RequestBody = "{}"
            };
            Task.Run(async () =>
            {
                ApiRequest.ApiResponse apiResp = await apiReq.StartRequestAsync("calendar/get_pending");
                GetEventsResult(apiResp, resultDelegate);
            });
        }

        public void GetEventsAsync(List<YearMonth> months, GetEventsResultDelegate resultDelegate)
        {
            if (IsActive == false)
                return;

            JObject jObj = JObject.FromObject(new { months = months });
            ApiRequest apiReq = new ApiRequest(_settings.AuthorizationTokenId, _settings.AuthorizationToken)
            {
                RequestBody = jObj.ToString()
            };
            Task.Run(async () =>
            {
                ApiRequest.ApiResponse apiResp = await apiReq.StartRequestAsync("calendar/get");
                GetEventsResult(apiResp, resultDelegate);
            });
        }

        private void GetEventsResult(ApiRequest.ApiResponse response, GetEventsResultDelegate resultDelegate)
        {
            bool result = response.IsSuccess;
            string reason = "";
            List<CalendarEvent> events = null;

            if (result)
            {
                try
                {
                    events = response.ResponseObject["events"].ToObject<List<CalendarEvent>>();
                }
                catch (Exception e)
                {
                    events = null;
                    reason = e.Message;
                }
            }
            else
            {
                reason = response.StatusText;
            }

            if (resultDelegate != null)
                resultDelegate(events, reason);            
        }

        public void AddEventAsync(CalendarEvent eventData, ModifyEventResultDelegate resultDelegate)
        {
            if (IsActive == false)
                return;

            JObject jObj = new JObject();
            jObj["event"] = JObject.FromObject(eventData);
            ApiRequest apiReq = new ApiRequest(_settings.AuthorizationTokenId, _settings.AuthorizationToken)
            {
                RequestBody = jObj.ToString()
            };
            Task.Run(async () =>
            {
                ApiRequest.ApiResponse apiResp = await apiReq.StartRequestAsync("calendar/add_event");

                ModifyEventResult(apiResp, resultDelegate);
            });
        }

        public void EditEventAsync(CalendarEvent eventData, ModifyEventResultDelegate resultDelegate)
        {
            if (IsActive == false)
                return;

            JObject jObj = new JObject();
            jObj["event"] = JObject.FromObject(eventData);
            ApiRequest apiReq = new ApiRequest(_settings.AuthorizationTokenId, _settings.AuthorizationToken)
            {
                RequestBody = jObj.ToString()
            };
            Task.Run(async () =>
            {
                ApiRequest.ApiResponse apiResp = await apiReq.StartRequestAsync("calendar/edit_event");

                ModifyEventResult(apiResp, resultDelegate);
            });
        }

        private static void ModifyEventResult(ApiRequest.ApiResponse apiResp, ModifyEventResultDelegate resultDelegate)
        {
            if (resultDelegate == null)
                return;

            string reason = "";
            List<YearMonth> months = null;
            CalendarEvent modifiedEvent = null;

            if (apiResp.IsSuccess)
            {
                if (apiResp.ResponseObject["status"].ToObject<int>() == ApiRequest.StatusCustomBase + 1)
                {
                    string filteredTitle = null;
                    string filteredDescription = null;
                    try
                    {
                        filteredTitle = apiResp.ResponseObject["title"].ToObject<string>();
                        filteredDescription = apiResp.ResponseObject["description"].ToObject<string>();
                    }
                    catch (Exception e)
                    {
                        filteredTitle = null;
                        filteredDescription = null;
                        reason = e.Message;
                    }
                    resultDelegate(null, null, reason, filteredTitle, filteredDescription);
                    return;
                }

                try
                {
                    months = apiResp.ResponseObject["months"].ToObject<List<YearMonth>>();
                    modifiedEvent = apiResp.ResponseObject["event"].ToObject<CalendarEvent>();
                }
                catch (Exception e)
                {
                    months = null;
                    modifiedEvent = null;
                    reason = e.Message;
                }
            }
            else
            {
                reason = apiResp.StatusText;
            }

            resultDelegate(modifiedEvent, months, reason, null, null);
        }

        public void ReplyInvitationAsync(UInt64 eventId, bool accept, ReplyInvitationDelegate resultDelegate)
        {
            if (IsActive == false)
                return;

            JObject jObj = JObject.FromObject(new { event_id = eventId, accept = accept });
            ApiRequest apiReq = new ApiRequest(_settings.AuthorizationTokenId, _settings.AuthorizationToken)
            {
                RequestBody = jObj.ToString()
            };
            Task.Run(async () =>
            {
                ApiRequest.ApiResponse apiResp = await apiReq.StartRequestAsync("calendar/reply_invitation");

                CalendarEvent eventData = null;
                string reason = apiResp.StatusText;

                try
                {
                    eventData = apiResp.ResponseObject["event"].ToObject<CalendarEvent>();
                }
                catch (Exception e)
                {
                    eventData = null;
                    reason = e.Message;
                }

                if (resultDelegate != null)
                    resultDelegate(eventData, reason);
            });
        }

        public void DeleteEventAsync(UInt64 eventId, DeleteEventResultDelegate resultDelegate)
        {
            if (IsActive == false)
                return;

            JObject jObj = JObject.FromObject(new { event_id = eventId });
            ApiRequest apiReq = new ApiRequest(_settings.AuthorizationTokenId, _settings.AuthorizationToken)
            {
                RequestBody = jObj.ToString()
            };
            Task.Run(async () =>
            {
                ApiRequest.ApiResponse apiResp = await apiReq.StartRequestAsync("calendar/delete_event");
                string reason = "Unknown.";
                
                if (!apiResp.IsSuccess)
                    reason = apiResp.StatusText;

                if (resultDelegate != null)
                    resultDelegate(apiResp.IsSuccess, reason);
            });
        }
    }
}
