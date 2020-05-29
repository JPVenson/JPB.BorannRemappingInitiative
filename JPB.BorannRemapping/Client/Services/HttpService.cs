using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Json;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Web;
using JPB.BorannRemapping.Shared.ViewModels;
using Newtonsoft.Json;

namespace JPB.BorannRemapping.Client.Services
{
	public class HttpService
	{
		private readonly HttpClient _httpClient;
		private JsonSerializerSettings _jsonSerializerSettings;
		private UserData _me;

		public UserData Me
		{
			get { return _me; }
			set
			{
				_me = value;
				OnMeChanged();
			}
		}

		public event EventHandler MeChanged;

		public HttpService(HttpClient httpClient)
		{
			_httpClient = httpClient;
			_jsonSerializerSettings = new JsonSerializerSettings()
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
			};
		}

		public async Task RefreshMe()
		{
			Me = await GetValue<UserData>("/api/CommanderApi/Me");
		}

		public static string EncodeUrlString(object data)
		{
			if (data is string dataString)
			{
				return HttpUtility.UrlPathEncode(dataString);
			}

			Console.WriteLine("D" + data);
			var values = data.GetType().GetProperties()
				.Select(e => new Tuple<string, object>(e.Name, e.GetMethod.Invoke(data, null)))
				.Where(e => e.Item2 != null);
			Console.WriteLine("Items: " + values.Count());
			foreach (var value in values)
			{
				Console.WriteLine(value.Item1 + " - " + value.Item2);
			}

			return values
				.Select(e => Uri.EscapeDataString(e.Item1) + "=" + Uri.EscapeDataString(e.Item2.ToString()))
				.Aggregate((e, f) => e + "&" + f);
		}

		public static string BuildUrl(string basePath, object data = null)
		{
			if (data == null)
			{
				return basePath;
			}

			return basePath + "?" + EncodeUrlString(data);
		}

		public async Task<ApiResult<T>> Get<T>(string url
			//, object body = null
		)
		{
			HttpResponseMessage content = null;
			try
			{
				var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
				//if (body != null)
				//{
				//	requestMessage.Content = new ObjectContent(body.GetType(), body, new JsonMediaTypeFormatter()
				//	{
				//		SerializerSettings = _jsonSerializerSettings
				//	});
				//}

				using (content = await _httpClient
					.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead)
					.ConfigureAwait(false))
				{
					if (!content.IsSuccessStatusCode)
					{
						return new ApiResult<T>(content.StatusCode, false, 
							await content.Content.ObtainMessage(), null);
					}

					return new ApiResult<T>(content.StatusCode, content.IsSuccessStatusCode,
						await content.Content.ReadFromJsonAsync<T>(), content.ReasonPhrase);
				}
			}
			catch (Exception e)
			{
				return new ApiResult<T>(content?.StatusCode ?? HttpStatusCode.ServiceUnavailable,
					false,
					content?.ReasonPhrase,
					ExceptionDispatchInfo.Capture(e));
			}
		}

		public async Task<T> GetValue<T>(string url)
		{
			return (await Get<T>(url)).Object;
		}

		public async Task<ApiResult> Post<T>(string url, T data)
		{
			HttpResponseMessage content = null;
			try
			{
				using (content = await _httpClient
					.PostAsync(url, new ObjectContent(data.GetType(), data, new JsonMediaTypeFormatter()
					{
						SerializerSettings = _jsonSerializerSettings
					}))
					.ConfigureAwait(false))
				{
					if (!content.IsSuccessStatusCode)
					{
						return new ApiResult(content.StatusCode, false, await content.Content.ObtainMessage(), null);
					}

					return new ApiResult(content.StatusCode, content.IsSuccessStatusCode, content.ReasonPhrase);
				}
			}
			catch (Exception e)
			{
				return new ApiResult(content?.StatusCode ?? HttpStatusCode.ServiceUnavailable,
					false,
					content?.ReasonPhrase,
					ExceptionDispatchInfo.Capture(e));
			}
		}

		public async Task<ApiResult> Post(string url)
		{
			HttpResponseMessage content = null;
			try
			{
				using (content = await _httpClient.PostAsync(url, new MultipartContent())
					.ConfigureAwait(false))
				{
					if (!content.IsSuccessStatusCode)
					{
						return new ApiResult(content.StatusCode, false, await content.Content.ObtainMessage(), null);
					}
					return new ApiResult(content.StatusCode, content.IsSuccessStatusCode, content.ReasonPhrase);
				}
			}
			catch (Exception e)
			{
				return new ApiResult(content?.StatusCode ?? HttpStatusCode.ServiceUnavailable,
					false,
					content?.ReasonPhrase,
					ExceptionDispatchInfo.Capture(e));
			}
		}

		public async Task<ApiResult<TE>> Post<T, TE>(string url, T data)
		{
			HttpResponseMessage content = null;
			try
			{
				using (content = await _httpClient.PostAsync(url, new ObjectContent(data.GetType(), data, new JsonMediaTypeFormatter()
					{
						SerializerSettings = _jsonSerializerSettings
					}))
					.ConfigureAwait(false))
				{
					if (!content.IsSuccessStatusCode)
					{
						return new ApiResult<TE>(content.StatusCode, false, await content.Content.ObtainMessage(), null);
					}
					return new ApiResult<TE>(content.StatusCode,
						content.IsSuccessStatusCode,
						await content.Content.ReadAsAsync<TE>(),
						content.ReasonPhrase);
				}
			}
			catch (Exception e)
			{
				return new ApiResult<TE>(content?.StatusCode ?? HttpStatusCode.ServiceUnavailable,
					false,
					content?.ReasonPhrase,
					ExceptionDispatchInfo.Capture(e));
			}
		}

		protected virtual void OnMeChanged()
		{
			MeChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	public class HttpServiceAnonymous : HttpService
	{
		public HttpServiceAnonymous(HttpClient httpClient) : base(httpClient)
		{
		}
	}

	public interface IApiResult
	{
		HttpStatusCode StatusCode { get; }
		bool Success { get; }
		string StatusMessage { get; }
	}

	public static class HttpErrorExtentions
	{
		private class ErrorMessageClass
		{
			[JsonProperty("message")]
			public string Message { get; set; }
		}

		public static async Task<string> ObtainMessage(this HttpContent content)
		{
			var error = await content.ReadAsStringAsync();
			try
			{
				if (string.IsNullOrWhiteSpace(error))
				{
					return string.Empty;
				}
				return JsonConvert.DeserializeObject<ErrorMessageClass>(error).Message;
			}
			catch
			{
				return error;
			}
		}
	}

	public struct ApiResult : IApiResult
	{
		public ApiResult(HttpStatusCode statusCode, bool success, string statusMessage, ExceptionDispatchInfo exception = null)
		{
			StatusCode = statusCode;
			Success = success;
			StatusMessage = statusMessage;
			Exception = exception;
		}

		public HttpStatusCode StatusCode { get; private set; }
		public bool Success { get; private set; }
		public string StatusMessage { get; private set; }
		public ExceptionDispatchInfo Exception { get; }

		public ApiResult UnpackOrThrow()
		{
			if (Success)
			{
				return this;
			}

			Exception?.Throw();
			return this;
		}
	}

	public struct ApiResult<T> : IApiResult
	{
		public ApiResult(HttpStatusCode statusCode, bool success, string statusMessage, ExceptionDispatchInfo exception)
		{
			StatusCode = statusCode;
			Success = success;
			StatusMessage = statusMessage;
			Exception = exception;
			Object = default(T);
		}

		public ApiResult(HttpStatusCode statusCode, bool success, T o, string statusMessage)
		{
			StatusCode = statusCode;
			Success = success;
			Object = o;
			StatusMessage = statusMessage;
			Exception = null;
		}

		public T Object { get; private set; }
		public HttpStatusCode StatusCode { get; private set; }
		public bool Success { get; private set; }
		public string StatusMessage { get; private set; }
		public ExceptionDispatchInfo Exception { get; }

		public ApiResult<T> UnpackOrThrow()
		{
			if (Success)
			{
				return this;
			}

			Exception?.Throw();
			return this;
		}
	}
}
