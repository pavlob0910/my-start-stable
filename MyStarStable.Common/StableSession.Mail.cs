using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MyStarStable.Common
{
    public sealed partial class StableSession
    {
        public delegate void GetMailResultDelegate(List<CharacterMailMessage> messages, string reason);
        public delegate void DeleteMailResultDelegate(bool result, string reason);
        public delegate void SendMailResultDelegate(bool result, string reason, string subject, string message);
        public delegate void MarkMailReadResultDelegate(bool result, string reason);

        public void GetMailAsync(GetMailResultDelegate resultDelegate)
        {
            if (IsActive == false)
                return;

            ApiRequest apiReq = new ApiRequest(_settings.AuthorizationTokenId, _settings.AuthorizationToken)
            {
                RequestBody = "{}"
            };
            Task.Run(async () =>
            {
                ApiRequest.ApiResponse apiResp = await apiReq.StartRequestAsync("mail/get");
                GetMailResult(apiResp, resultDelegate);
            });
        }

        private void GetMailResult(ApiRequest.ApiResponse response, GetMailResultDelegate resultDelegate)
        {
            bool result = response.IsSuccess;
            string reason = "";
            List<CharacterMailMessage> messages = null;

            if (result)
            {
                try
                {
                    messages = response.ResponseObject["messages"].ToObject<List<CharacterMailMessage>>();
                }
                catch (Exception e)
                {
                    messages = null;
                    reason = e.Message;
                }
            }
            else
            {
                reason = response.StatusText;
            }

            if (resultDelegate != null)
                resultDelegate(messages, reason);
        }

        public void DeleteMailAsync(UInt64 mailId, DeleteMailResultDelegate resultDelegate)
        {
            if (IsActive == false)
                return;

            JObject jObj = JObject.FromObject(new { mail_id = mailId });
            ApiRequest apiReq = new ApiRequest(_settings.AuthorizationTokenId, _settings.AuthorizationToken)
            {
                RequestBody = jObj.ToString()
            };
            Task.Run(async () =>
            {
                ApiRequest.ApiResponse apiResp = await apiReq.StartRequestAsync("mail/delete");
                
                bool result = apiResp.IsSuccess;
                string reason = "Unknown.";

                if (!result)
                {
                    reason = apiResp.StatusText;
                }

                if (resultDelegate != null)
                    resultDelegate(result, reason);
            });
        }

        public void SendMailAsync(string recipient, string subject, string message, SendMailResultDelegate resultDelegate)
        {
            if (IsActive == false)
                return;

            JObject jObj = JObject.FromObject(new { recipient = recipient, subject = subject, message = message });
            ApiRequest apiReq = new ApiRequest(_settings.AuthorizationTokenId, _settings.AuthorizationToken)
            {
                RequestBody = jObj.ToString()
            };
            Task.Run(async () =>
            {
                ApiRequest.ApiResponse apiResp = await apiReq.StartRequestAsync("mail/send");

                if (resultDelegate == null)
                    return;

                bool result = apiResp.IsSuccess;
                string reason = "";

                if (result)
                {
                    if (apiResp.ResponseObject["status"].ToObject<int>() == ApiRequest.StatusCustomBase + 1)
                    {
                        string filteredSubject = null;
                        string filteredMessage = null;
                        try
                        {
                            filteredSubject = apiResp.ResponseObject["subject"].ToObject<string>();
                            filteredMessage = apiResp.ResponseObject["message"].ToObject<string>();
                        }
                        catch (Exception e)
                        {
                            result = false;
                            reason = e.Message;
                        }
                        resultDelegate(result, reason, filteredSubject, filteredMessage);
                        return;
                    }
                }
                else 
                { 
                    reason = apiResp.StatusText;
                }

                resultDelegate(result, reason, null, null);
            });
        }

        public void MarkMailReadAsync(UInt64 mailId, MarkMailReadResultDelegate resultDelegate)
        {
            if (IsActive == false)
                return;

            JObject jObj = JObject.FromObject(new { mail_id = mailId });
            ApiRequest apiReq = new ApiRequest(_settings.AuthorizationTokenId, _settings.AuthorizationToken)
            {
                RequestBody = jObj.ToString()
            };
            Task.Run(async () =>
            {
                ApiRequest.ApiResponse apiResp = await apiReq.StartRequestAsync("mail/mark_read");

                if (resultDelegate == null)
                    return;

                bool result = apiResp.IsSuccess;
                string reason = "Unknown.";

                if (!result)
                {
                    reason = apiResp.StatusText;
                }

                resultDelegate(result, reason);
            });
        }

        public void ValidateNameAsync(string characterName, ValidateNameResultDelegate resultDelegate)
        {
            if (IsActive == false)
                return;

            string[] names = characterName.Split((char [])null, StringSplitOptions.RemoveEmptyEntries);

            if (names.Length < 2)
            {
                if (resultDelegate != null)
                {
                    Task.Run(() => resultDelegate(false, "Invalid name."));
                }
                return;
            }

            JObject jObj = JObject.FromObject(new { first_name = names[0], last_name = names[1] });
            ApiRequest apiReq = new ApiRequest(_settings.AuthorizationTokenId, _settings.AuthorizationToken)
            {
                RequestBody = jObj.ToString()
            };
            Task.Run(async () =>
            {
                ApiRequest.ApiResponse apiResp = await apiReq.StartRequestAsync("character/validate_name");

                if (resultDelegate == null)
                    return;

                bool result = apiResp.IsSuccess;
                string reason = "Unknown.";

                if (result)
                {
                    try
                    {
                        bool nameValid = apiResp.ResponseObject["valid"].ToObject<bool>();
                        result = nameValid;
                    }
                    catch (Exception e)
                    {
                        result = false;
                        reason = e.Message;
                    }
                }
                else
                {
                    reason = apiResp.StatusText;
                }

                resultDelegate(result, reason);
            });
        }
    }
}
