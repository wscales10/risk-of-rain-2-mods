using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Utils;

namespace Spotify.Authorisation
{
	public delegate Task<HttpResponseMessage> Sender(HttpRequestMessage request);

	internal class CodeFlow
	{
		protected readonly App app;

		private ErrorState errorState;

		private FlowState state;

		public CodeFlow(App app)
		{
			using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
			{
				byte[] stateData = new byte[12];
				rng.GetBytes(stateData);
				StateString = Convert.ToBase64String(stateData);
			}

			this.app = app;
		}

		public string Code { get; private set; }

		public ErrorState ErrorState
		{
			get => errorState;

			private set
			{
				if ((errorState = value) == ErrorState.None)
				{
					throw new InvalidOperationException();
				}
				else
				{
					state = FlowState.Error;
				}
			}
		}

		public string RefreshToken { get; private set; }

		public FlowState State
		{
			get => state;

			private set
			{
				if ((state = value) == FlowState.Error)
				{
					throw new InvalidOperationException();
				}
				else
				{
					errorState = ErrorState.None;
				}
			}
		}

		public string StateString { get; }

		public bool IsFaulted => State == FlowState.Error;

		public virtual NameValueCollection GetLoginQueryString(IEnumerable<string> scopes)
		{
			State = FlowState.Login;
			var output = HttpUtility.ParseQueryString(string.Empty);
			output.Add("response_type", "code");
			output.Add("client_id", app.ClientId);
			output.Add("scope", string.Join(" ", scopes));
			output.Add("redirect_uri", app.RedirectUri.ToString());
			output.Add("state", StateString);
			return output;
		}

		private void ThrowIfFaulted()
		{
			if (IsFaulted)
			{
				throw new InvalidOperationException();
			}
		}

		public async Task<RefreshTokenInfo> RequestTokensAsync(Sender send)
		{
			ThrowIfFaulted();

			using (var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token"))
			{
				request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ app.ClientId }:{ app.ClientSecret }")));
				Dictionary<string, string> nameValueCollection = GetAccessTokenRequest();
				request.Content = new FormUrlEncodedContent(nameValueCollection);
				var responseMessage = await send(request);
				var deserialised = await Deserialize<RefreshTokenInfo>(responseMessage);

				if (deserialised is null)
				{
					State = FlowState.Error;
				}
				else
				{
					State = FlowState.TokenGranted;
					RefreshToken = deserialised.RefreshToken;
				}

				return deserialised;
			}
		}

		public async Task<AccessTokenInfo> TryRefreshTokenAsync(Sender send)
		{
			ThrowIfFaulted();
			State = FlowState.Refreshing;

			using (var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token"))
			{
				request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{app.ClientId}:{ app.ClientSecret }")));
				request.Content = new FormUrlEncodedContent(GetRefreshTokenRequest());
				var responseMessage = await send(request);
				var deserialised = await Deserialize<AccessTokenInfo>(responseMessage);

				if (deserialised is null)
				{
					State = FlowState.Error;
				}
				else
				{
					State = FlowState.TokenRefreshed;
				}

				return deserialised;
			}
		}

		public bool TryTransitionToTokenRequestState(string state, string code, ref string error)
		{
			if (state != StateString)
			{
				ErrorState = ErrorState.StateMismatch;
				error = "state mismatch";
				return false;
			}

			if(error == "access_denied")
			{
				ErrorState = ErrorState.UserDenied;
				return false;
			}

			if (code is null)
			{
				State = FlowState.Error;
				return false;
			}

			State = FlowState.TokenRequest;
			Code = code;
			return true;
		}

		protected virtual Dictionary<string, string> GetAccessTokenRequest() => new Dictionary<string, string>()
			{
				{"grant_type", "authorization_code" },
				{ "code", Code },
				{"redirect_uri", app.RedirectUri.ToString() }
			};

		protected virtual Dictionary<string, string> GetRefreshTokenRequest() => new Dictionary<string, string>()
			{
				{"grant_type", "refresh_token" },
				{ "refresh_token", RefreshToken },
			};

		private async Task<T> Deserialize<T>(HttpResponseMessage responseMessage)
			where T : class
		{
			var content = responseMessage.Content;
			var str = await content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<T>(str, new JsonSerializerSettings { Error = HandleDeserializationError });
		}

		private void HandleDeserializationError(object sender, ErrorEventArgs errorArgs)
		{
			this.Log(errorArgs.ErrorContext.Error.Message);
			errorArgs.ErrorContext.Handled = true;
		}
	}
}