using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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

				res.ContentType = "text/html";
				var buffer = File.ReadAllBytes(Paths.AssemblyDirectory + "/Authorisation/Client/RequestTokens.html");
				res.ContentLength64 = buffer.Length;
				await res.OutputStream.WriteAsync(buffer, 0, buffer.Length);
				res.OutputStream.Close();

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