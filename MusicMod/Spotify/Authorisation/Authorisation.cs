using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using Utils.Reflection;

namespace Spotify.Authorisation
{
    public delegate Task<bool> AsyncBoolCallback(HttpListenerRequest request, HttpListenerResponse response);

    public delegate Task AsyncCallback(HttpListenerRequest request, HttpListenerResponse response);

    public delegate bool BoolCallback(HttpListenerRequest request, HttpListenerResponse response);

    public delegate void VoidCallback(HttpListenerRequest request, HttpListenerResponse response);

    public partial class Authorisation : AsyncRunner
    {
        private static readonly HttpClient client = new HttpClient();

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private readonly Func<CodeFlow> getFlow;

        private readonly Dictionary<string, bool> scopes = new Dictionary<string, bool>();

        private readonly Server server;

        private JoinableTask refreshTask;

        private AccessTokenInfo data;

        private CodeFlow flow;

        private DateTime refreshBy;

        public Authorisation(IEnumerable<string> scopes, bool isPkceEnabled = true, Logger logger = null)
        {
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

        public Func<int, TimeSpan> GetWaitTime { get; set; } = expiresIn => TimeSpan.FromSeconds(Math.Max(expiresIn - 300, expiresIn / 2));

        private DateTime RefreshBy
        {
            get
            {
                return refreshBy;
            }

            set
            {
                refreshBy = value;
                this.LogPropertyValue(value);
            }
        }

        public void OpenConfigurationPage() => OnClientRequested?.Invoke(App.Instance.RootUri);

        public async Task InitiateScopeRequestAsync()
        {
            _ = await TryStartAsync();
            OnClientRequested?.Invoke(App.Instance.RootUri);
        }

        protected override async Task PauseAsync()
        {
            if (flow?.State < FlowState.TokenGranted)
            {
                return;
            }

            cancellationTokenSource.Cancel();
            await server.TryPauseAsync();
            await refreshTask;
        }

        protected override async Task StopAsync()
        {
            cancellationTokenSource.Cancel();
            await server.TryStopAsync();
            await refreshTask;
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

                data = await flow.RequestTokensAsync();

                if (data is null || flow.State == FlowState.Error)
                {
                    flow = null;
                    return;
                }

                OnAccessTokenReceived?.Invoke(this, data.AccessToken);
                RefreshIn(GetWaitTime(data.ExpiresIn));
                refreshTask = Async.Manager.RunSafely(RefreshLoop);
            });

            server.On("POST", "/shutdown", (req, res) =>
            {
                Console.WriteLine("Shutdown requested");
                _ = TryStopAsync();
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
                await MakeResponseAsync(res, Assembly.GetExecutingAssembly().GetByteArray("Spotify.Authorisation.Client.index.html"));
            });

            server.On("POST", "defaultDevice.json", async (req, res) =>
            {
                var arr = await GetRequestBodyAsync(req);
                Preferences.DefaultDevice = arr?.Length == 0 ? null : arr;
            });

            server.On("GET", "index.html", (req, res) =>
            {
                if (flow.State >= FlowState.TokenGranted)
                {
                    res.Redirect("index.html");
                }
            }, true);
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
    }
}