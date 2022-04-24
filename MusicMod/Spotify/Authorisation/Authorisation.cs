using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using Utils.Async;

namespace Spotify.Authorisation
{
	public delegate Task<bool> AsyncBoolCallback(HttpListenerRequest request, HttpListenerResponse response);

	public delegate Task AsyncCallback(HttpListenerRequest request, HttpListenerResponse response);

	public delegate bool BoolCallback(HttpListenerRequest request, HttpListenerResponse response);

	public delegate void VoidCallback(HttpListenerRequest request, HttpListenerResponse response);

	public partial class Authorisation
	{
		private static readonly HttpClient client = new HttpClient();

		private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

		private readonly Func<CodeFlow> getFlow;

		private readonly AsyncManualResetEvent pauseEvent = new AsyncManualResetEvent(true);

		private readonly Dictionary<string, bool> scopes = new Dictionary<string, bool>();

		private readonly Server server;

		private readonly AsyncManualResetEvent stopEvent = new AsyncManualResetEvent(true);

		private AccessTokenInfo data;

		private CodeFlow flow;

		private DateTime refreshBy;

		public Authorisation(IEnumerable<string> scopes, bool isPkceEnabled = true, Logger logger = null)
		{
			Lifecycle = new SingletonTaskWithSetup(
				StartInner,
				async () => await stopEvent);
			refresh = new SingletonTask(RefreshLoop);
			stop = new SingletonTask(StopInner);
			server = new Server(App.Instance.RootUri, logger);

			if (isPkceEnabled)
			{
				getFlow = () => new PkceFlow(App.Instance);
			}
			else
			{
				getFlow = () => new CodeFlow(App.Instance);
			}

			SetupServer();

			foreach (var scope in scopes)
			{
				this.scopes[scope] = false;
			}
		}

		public delegate void AccessTokenHandler(Authorisation sender, string accessToken);

		public event AccessTokenHandler OnAccessTokenReceived;

		public event Action<Uri> OnClientRequested;

		private bool IsPaused => pauseEvent.IsSet;

		public void InitiateScopeRequest()
		{
			TryStart();
			OnClientRequested?.Invoke(App.Instance.RootUri);
		}

		public async Task PauseAsync()
		{
			if (flow?.State != FlowState.TokenGranted)
			{
				return;
			}

			if (IsPaused)
			{
				return;
			}

			pauseEvent.Reset();
			cancellationTokenSource.Cancel();
			await server.StopAsync();
			await refresh.Task;
			refresh.Start();
		}

		public void Resume()
		{
			if (IsPaused)
			{
				_ = server.ListenAsync();
				pauseEvent.Set();
			}
		}

		public async Task StopAsync()
		{
			await stop.RunAsync();
		}

		private static async Task MakeResponseAsync(HttpListenerResponse res, byte[] byteArray)
		{
			res.ContentLength64 = byteArray?.Length ?? 0;
			if (!(byteArray is null))
			{
				await res.OutputStream.WriteAsync(byteArray, 0, byteArray.Length);
			}
			else
			{
				res.StatusCode = (int)HttpStatusCode.NoContent;
			}
			res.OutputStream.Close();
		}

		private void SetupServer()
		{
			server.On("GET", "/login", (req, res) =>
			{
				if (flow.State == FlowState.None)
				{
					res.Redirect(App.Instance.RootUri + "login");
					return false;
				}

				return true;
			}, true);

			server.On("GET", "/login", (req, res) =>
			{
				var queryString = flow.GetLoginQueryString(scopes.Keys.Where(k => !scopes[k]));
				res.Redirect("https://accounts.spotify.com/authorize?" + queryString);
			});

			server.On("GET", "/callback", async (req, res) =>
			{
				string error = req.QueryString["error"];

				if (!flow.TryTransitionToTokenRequestState(req.QueryString["state"], req.QueryString["code"], ref error))
				{
					this.Log(error);
					res.StatusCode = (int)HttpStatusCode.Unauthorized;
					flow = null;
					return;
				}

				res.Redirect("index.html");

				data = await flow.RequestTokensAsync(async (m) => await client.SendAsync(m));

				if (data is null)
				{
					flow = null;
					return;
				}

				OnAccessTokenReceived?.Invoke(this, data.AccessToken);

				refreshBy = DateTime.UtcNow + TimeSpan.FromSeconds(Math.Max(data.ExpiresIn - 300, data.ExpiresIn / 2));

				refresh.Start();
			});

			server.On("POST", "/shutdown", (req, res) =>
			{
				Console.WriteLine("Shutdown requested");
				stop.Start();
			});

			server.On("GET", "devices.json", async (req, res) =>
			{
				res.ContentType = "application/json";
				var content = await GetDevicesAsync();
				await MakeResponseAsync(res, await content.ReadAsByteArrayAsync());
			});

			server.On("GET", "defaultDevice.json", async (req, res) =>
			{
				res.ContentType = "application/json";
				var content = Preferences.DefaultDevice;
				await MakeResponseAsync(res, content?.Length == 0 ? null : content);
			});

			server.On("GET", "index.html", async (req, res) =>
			{
				res.ContentType = "text/html";
				await MakeResponseAsync(res, File.ReadAllBytes(Paths.AssemblyDirectory + "/Authorisation/Client/index.html"));
			});

			server.On("POST", "defaultDevice.json", async (req, res) =>
			{
				var arr = await GetRequestBodyAsync(req);
				Preferences.DefaultDevice = arr?.Length == 0 ? null : arr;
			});
		}

		private async Task<byte[]> GetRequestBodyAsync(HttpListenerRequest req)
		{
			int len = (int)req.ContentLength64;
			byte[] buffer = new byte[len];
			await req.InputStream.ReadAsync(buffer, 0, len);
			return buffer;
		}

		private async Task<HttpContent> GetDevicesAsync()
		{
			using (var request = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me/player/devices"))
			{
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", data.AccessToken);
				var responseMessage = await client.SendAsync(request);
				return responseMessage.Content;
			}
		}

		private void Start()
		{
			Lifecycle.Start();
		}

		private void TryStart()
		{
			if (!Lifecycle.IsRunning)
			{
				Start();
			}
		}
	}
}