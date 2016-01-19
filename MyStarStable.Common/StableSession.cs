using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PCLStorage;

namespace MyStarStable.Common
{
    public sealed partial class StableSession
    {
        private static Lazy<StableSession> _lazyInstance = new Lazy<StableSession>(() => new StableSession());

        public static StableSession Instance
        {
            get { return _lazyInstance.Value; }
        }

        public static string ApiGuid;
        public static string DeviceId;
        public static byte[] DevicePushToken;

        public static void Reset()
        {
            _lazyInstance = new Lazy<StableSession>(() => new StableSession());
        }

        public bool IsActive
        {
            get;
            private set;
        }

        public UserData CurrentUserData;

        public delegate void RestoreResultDelegate(bool result);

        public enum LoginResult
        {
            Ok,
            Failed,
            NeedPassword
        }
        public delegate void LoginResultDelegate(LoginResult result, string reason);
        public delegate void ValidateNameResultDelegate(bool result, string reason);
        public delegate void GetFriendsResultDelegate(List<CharacterFriend> friends, string reason);

        private SettingsObject _settings;

        private StableSession()
        {
            if (LoadSettings() == false)
            {
                _settings = new SettingsObject();
            }
            CurrentUserData = null;
        }

        private void ClearTokens()
        {
            if (_settings.AuthorizationTokenId > 0 || _settings.AuthorizationToken.Length > 0)
            {
                _settings.AuthorizationToken = new byte[0];
                _settings.AuthorizationTokenId = 0;
                SaveSettings();
            }
        }

        public void Close(bool clearTokens)
        {
            if (clearTokens)
                ClearTokens();

            IsActive = false;
        }

        #region Settings
        /// <summary>
        /// Saves the Settings class JSon dump to "Settings/starstable" using PCLStorage.
        /// </summary>
        /// <returns>true on success.</returns>
        private bool SaveSettings()
        {
            IFolder rootFolder = FileSystem.Current.LocalStorage;

            IFolder settingsFolder = null;
            if (rootFolder.CheckExistsAsync("Settings").Result != ExistenceCheckResult.FolderExists)
            {
                settingsFolder = rootFolder.CreateFolderAsync("Settings", CreationCollisionOption.FailIfExists).Result;
            }
            else
            {
                settingsFolder = rootFolder.GetFolderAsync("Settings").Result;
            }

            if (settingsFolder == null)
                return false;


            IFile settingsFile = null;
            if (settingsFolder.CheckExistsAsync("starstable").Result != ExistenceCheckResult.FileExists)
            {
                settingsFile = settingsFolder.CreateFileAsync("starstable", CreationCollisionOption.FailIfExists).Result;
            }
            else
            {
                settingsFile = settingsFolder.GetFileAsync("starstable").Result;
            }

            if (settingsFile == null)
                return false;

            string settingsText = JsonConvert.SerializeObject(_settings);
            settingsFile.WriteAllTextAsync(settingsText).Wait();

            return true;
        }

        private bool LoadSettings()
        {
            IFolder rootFolder = FileSystem.Current.LocalStorage;
            if (rootFolder.CheckExistsAsync("Settings").Result != ExistenceCheckResult.FolderExists)
            {
                _settings = null;
                return false;
            }

            IFolder settingsFolder = rootFolder.GetFolderAsync("Settings").Result;
            if (settingsFolder.CheckExistsAsync("starstable").Result != ExistenceCheckResult.FileExists)
            {
                _settings = null;
                return false;
            }
            IFile settingsFile = settingsFolder.GetFileAsync("starstable").Result;
            
            string settingsText = Task.Run(async () => await settingsFile.ReadAllTextAsync()).Result;
            _settings = JsonConvert.DeserializeObject<SettingsObject>(settingsText);
            if (_settings == null)
                return false;

            return true;
        }
        #endregion

        #region Login
        private async Task<Tuple<UserData, string>> GetUserData()
        {
            ApiRequest apiReq = new ApiRequest(_settings.AuthorizationTokenId, _settings.AuthorizationToken)
            {
                RequestBody = "{}"
            };
            ApiRequest.ApiResponse apiResp = await apiReq.StartRequestAsync("user_data");

            UserData user = null;
            string reason = apiResp.StatusText;

            if (apiResp.IsSuccess)
            {
                try
                {
                    user = apiResp.ResponseObject["user"].ToObject<UserData>();
                }
                catch (Exception ex)
                {
                    user = null;
                    reason = ex.Message;
                }
            }
            return new Tuple<UserData, string>(user, reason);
        }

        /// <summary>
        /// Tries to verify the saved token with the server.
        /// </summary>
        /// <param name="resultDelegate">Delegate that is called when operation is finished.</param>
        public void RestoreAsync(RestoreResultDelegate resultDelegate)
        {
            IsActive = false;

            if (_settings.AuthorizationTokenId > 0 && _settings.AuthorizationToken.Length > 0)
            {
                ApiRequest apiReq = new ApiRequest(_settings.AuthorizationTokenId, _settings.AuthorizationToken)
                {
                    RequestBody = "{}"
                };
                Task.Run(async () =>
                {
                    ApiRequest.ApiResponse apiResp = await apiReq.StartRequestAsync("verify_token");

                    if (apiResp.IsSuccess)
                    {
                        Tuple<UserData, string> userData = await GetUserData();
                        CurrentUserData = userData.Item1;
                        IsActive = (CurrentUserData != null);
                    }

                    if (IsActive)
                    {
                        UpdatePushTokenAsync();
                    }
                    else
                    {
                        ClearTokens();
                    }

                    if (resultDelegate != null)
                        resultDelegate(IsActive);
                });
            }
            else
            {
                if (resultDelegate != null)
                    Task.Run(() => resultDelegate(false));
            }
        }

        public void FacebookLoginAsync(string facebookId, string facebookToken, string linkUser, string linkPassword, LoginResultDelegate resultDelegate)
        {
            IsActive = false;

            JObject jObj = JObject.FromObject(new
            {
                facebook_id = facebookId,
                facebook_token = facebookToken,
                link_user = linkUser,
                link_password = linkPassword,
                device_id = DeviceId,
                api_guid = ApiGuid
            });

            GetTokenAsync(jObj, resultDelegate);
        }

        public void LoginAsync(string user, string password, LoginResultDelegate resultDelegate)
        {
            IsActive = false;

            JObject jObj = JObject.FromObject(new
            {
                username = user, 
                password = password, 
                device_id = DeviceId,
                api_guid = ApiGuid
            });

            GetTokenAsync(jObj, resultDelegate);
        }

        private void GetTokenAsync(JObject paramsObject, LoginResultDelegate resultDelegate)
        {
            ApiRequest apiReq = new ApiRequest { RequestBody = paramsObject.ToString() };
            Task.Run(async () =>
            {
                ApiRequest.ApiResponse apiResp = await apiReq.StartRequestAsync("get_token");

                Tuple<LoginResult, string> loginResult = ParseTokenResult(apiResp);

                if (loginResult.Item1 == LoginResult.Ok)
                {
                    Tuple<UserData, string> userData = await GetUserData();
                    CurrentUserData = userData.Item1;
                    IsActive = (CurrentUserData != null);
                    if (IsActive)
                    {
                        SaveSettings();
                        UpdatePushTokenAsync();
                        resultDelegate(LoginResult.Ok, userData.Item2);
                    }
                    else
                    {
                        ClearTokens();
                        resultDelegate(LoginResult.Failed, userData.Item2);
                    }
                }
                else
                {
                    ClearTokens();
                    resultDelegate(loginResult.Item1, loginResult.Item2);
                }
            });
        }

        private Tuple<LoginResult, string> ParseTokenResult(ApiRequest.ApiResponse apiResp)
        {
            if (!apiResp.IsSuccess)
            {
                return new Tuple<LoginResult, string>(LoginResult.Failed, apiResp.StatusText);
            }

            // Status already exists, this is checked by ApiRequest
            if (apiResp.ResponseObject["status"].ToObject<int>() == ApiRequest.StatusCustomBase + 1)
            {
                return new Tuple<LoginResult, string>(LoginResult.NeedPassword, apiResp.StatusText);
            }

            try
            {
                UInt64 tokenid = apiResp.ResponseObject["tokenid"].ToObject<UInt64>();
                string token = apiResp.ResponseObject["token"].ToObject<string>();
                _settings.AuthorizationTokenId = tokenid;
                _settings.AuthorizationToken = Base64.Decode(token);
            }
            catch (Exception ex)
            {
                return new Tuple<LoginResult, string>(LoginResult.Failed, ex.Message);
            }


            return new Tuple<LoginResult, string>(LoginResult.Ok, "");
        }

        private void UpdatePushTokenAsync()
        {
            if (IsActive == false || DevicePushToken == null)
                return;

            JObject jObj = JObject.FromObject(
                new { push_token = Base64.Encode(DevicePushToken), 
#if !DEBUG
                      platform_type = 2 /* iOS Sandbox */,
#else
                      platform_type = 1 /* iOS */, 
#endif
                });
            ApiRequest apiReq = new ApiRequest(_settings.AuthorizationTokenId, _settings.AuthorizationToken)
            {
                RequestBody = jObj.ToString()
            };
            Task.Run(async () =>
            {
                await apiReq.StartRequestAsync("update_push_token");
            });
        }
        #endregion

        #region Friends
        public void GetFriendsAsync(GetFriendsResultDelegate resultDelegate)
        {
            if (IsActive == false)
                return;

            ApiRequest apiReq = new ApiRequest(_settings.AuthorizationTokenId, _settings.AuthorizationToken)
            {
                RequestBody = "{}"
            };
            Task.Run(async () =>
            {
                ApiRequest.ApiResponse apiResp = await apiReq.StartRequestAsync("character/friends/get");
                GetFriendsResult(apiResp, resultDelegate);
            });
        }

        private void GetFriendsResult(ApiRequest.ApiResponse response, GetFriendsResultDelegate resultDelegate)
        {
            bool result = response.IsSuccess;
            string reason = "";
            List<CharacterFriend> friends = null;

            if (result)
            {
                try
                {
                    friends = response.ResponseObject["friends"].ToObject<List<CharacterFriend>>();
                }
                catch (Exception e)
                {
                    friends = null;
                    reason = e.Message;
                }
            }
            else
            {
                reason = response.StatusText;
            }

            if (resultDelegate != null)
                resultDelegate(friends, reason);
        }
        #endregion
    }
}
