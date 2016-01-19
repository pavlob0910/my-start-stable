using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using ModernHttpClient;
using Newtonsoft.Json.Linq;

namespace MyStarStable.Common
{
	public class ApiRequest
	{
		public class ApiResponse
		{
			public enum ResponseStatus
			{
				Ok,
				HttpError,
				ApiError,
				NetworkError
			}

			public ApiResponse()
			{
				Status = ResponseStatus.Ok;
			}


			public bool IsSuccess
			{
				get { return (Status == ResponseStatus.Ok); }
			}
			public ResponseStatus Status { get; set; }
			public string StatusText { get; set; }

			public JObject ResponseObject { get; set; }
			public int ApiStatusCode { get; set; }
			public HttpStatusCode HttpStatusCode { get; set; }
		}

		public string BaseUrl { get; set; }
		public string RequestBody { get; set; }

		public UInt64 AuthorizationTokenId { get; set; }
		public byte[] AuthorizationToken { get; set; }

		public int TimeoutSeconds { get; set; }
		public int Retries { get; set; }

		private const int StatusOk = 0;
		private const int StatusErr = 1;
		public const int StatusCustomBase = 5000;

		public ApiRequest(UInt64 tokenId = 0, byte[] token = null)
		{
			AuthorizationTokenId = tokenId;
			AuthorizationToken = token;

//			#if DEBUG
//			BaseUrl = "http://dev02.dev.int.starstable.com:7301/"; //...
//			#else
			BaseUrl = "https://hiddenhorse.starstable.com/";
//			#endif
			TimeoutSeconds = 60; //60 sekunders timeout?
			Retries = 3;
		}

		private void AddHeaders(HttpRequestMessage httpReq, string requestUrl)
		{
			if (AuthorizationTokenId != 0 && AuthorizationToken.Length > 0)
			{
				string date = DateTime.UtcNow.ToString("yyyyMMdd\\THHmmss\\Z");
				string message = date + "\n\n" + requestUrl + "\n\n" + RequestBody;
				byte[] token = Interfaces.Instance.HMAC256Interface.ComputeHash(AuthorizationToken,
					Encoding.UTF8.GetBytes(message));
				httpReq.Headers.Add("Authorization", String.Format("STARSTABLE-API TokenID={0}, Token={1}", AuthorizationTokenId, Base64.Encode(token)));
				httpReq.Headers.Add("X-API-Date", date);
			}            
		}

		private void ParseResponse(ref ApiResponse apiResponse, string respString)
		{
			try
			{
				JObject jObj = JObject.Parse(respString);

				apiResponse.ResponseObject = jObj;
			}
			catch (Exception ex)
			{
				// Parse failed.
				apiResponse.ResponseObject = null;
				apiResponse.Status = ApiResponse.ResponseStatus.ApiError;
				apiResponse.StatusText = ex.Message;
			}

			if (apiResponse.ResponseObject != null)
			{
				try
				{
					int apiStatus = apiResponse.ResponseObject["status"].ToObject<int>();
					apiResponse.ApiStatusCode = apiStatus;
					if (apiStatus != StatusOk && apiStatus < StatusCustomBase)
					{
						apiResponse.Status = ApiResponse.ResponseStatus.ApiError;
						apiResponse.StatusText = apiResponse.ResponseObject["error"].ToObject<string>();
					}
				}
				catch (Exception ex)
				{
					apiResponse.Status = ApiResponse.ResponseStatus.ApiError;
					apiResponse.StatusText = ex.Message;                    
				}
			}            
		}

		public async Task<ApiResponse> StartRequestAsync(string endpointName)
		{
			for (int retry = 0; retry < Retries; ++retry)
			{
				try
				{

					return await StartRequest(endpointName);
				}
				catch (TimeoutException /*ex*/)
				{
					// Timeout - retry.
				}
			}
			ApiResponse apiResponse = new ApiResponse();
			apiResponse.Status = ApiResponse.ResponseStatus.NetworkError;
			apiResponse.StatusText = String.Format("Communication timeout after {0} retries.", Retries);
			return apiResponse;
		}

		private async Task<ApiResponse> StartRequest(string endpointName)
		{
			ApiResponse apiResponse = new ApiResponse();

			if (Interfaces.Instance.NetworkTesterInterface == null || !Interfaces.Instance.NetworkTesterInterface.HasInternet)
			{
				apiResponse.Status = ApiResponse.ResponseStatus.NetworkError;
				apiResponse.StatusText = "Network unavailable.";
				return apiResponse;
			}

			string requestUrl = BaseUrl + endpointName;

			
			using (HttpClient httpClient = new HttpClient(new NativeMessageHandler(false,true)) {Timeout = TimeSpan.FromSeconds(TimeoutSeconds)})
			{
				
				using (HttpRequestMessage httpReq = new HttpRequestMessage(HttpMethod.Post, requestUrl))
				{
					
					AddHeaders(httpReq, requestUrl);
					httpReq.Content = new StringContent(RequestBody, Encoding.UTF8, "application/json");

					HttpResponseMessage httpResp = null;
					string respString = null;
						
					try
					{
						
						httpResp = await httpClient.SendAsync(httpReq);
						respString = await httpResp.Content.ReadAsStringAsync();
					}
					catch (WebException ex)
					{
						apiResponse.Status = ApiResponse.ResponseStatus.NetworkError;
						apiResponse.StatusText = ex.GetBaseException().Message;
						return apiResponse;
					}
					catch (TaskCanceledException ex)
					{
						throw new TimeoutException("Request timed out.", ex);
					}

					if (httpResp.IsSuccessStatusCode)
					{
						ParseResponse(ref apiResponse, respString);
					}
					else
					{
						apiResponse.Status = ApiResponse.ResponseStatus.HttpError;
						apiResponse.HttpStatusCode = httpResp.StatusCode;
						apiResponse.StatusText = httpResp.ReasonPhrase;
					}
				}
			}

			return apiResponse;
		}
	}
}
