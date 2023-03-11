using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using Utils.Async;
using Utils.Runners;

namespace Utils
{
	public class HttpServer : AsyncRunner
	{
		private readonly Dictionary<string, List<(string, AsyncBoolCallback, bool)>> callbacks = new Dictionary<string, List<(string, AsyncBoolCallback, bool)>>();

		private readonly Logger Log;

		private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

		private readonly UriBuilder rootUri;

		private HttpListener listener = new HttpListener();

		private bool runServer;

		public HttpServer(Uri rootUri, Logger logger)
		{
			this.rootUri = new UriBuilder(rootUri);
			Log = logger;
			callbacks["GET"] = new List<(string, AsyncBoolCallback, bool)>();
			callbacks["POST"] = new List<(string, AsyncBoolCallback, bool)>();
		}

		public delegate Task<bool> AsyncBoolCallback(HttpListenerRequest request, HttpListenerResponse response);

		public delegate Task AsyncCallback(HttpListenerRequest request, HttpListenerResponse response);

		public delegate bool BoolCallback(HttpListenerRequest request, HttpListenerResponse response);

		public delegate void VoidCallback(HttpListenerRequest request, HttpListenerResponse response);

		public Task ListenTask { get; private set; }

		public Uri RootUri => rootUri.Uri;

		public static async Task MakeResponseAsync(HttpListenerResponse res, string message)
		{
			await MakeResponseAsync(res, message is null ? null : Encoding.UTF8.GetBytes(message));
		}

		public static async Task MakeResponseAsync(HttpListenerResponse res, byte[] byteArray)
		{
			res.ContentLength64 = byteArray?.Length ?? 0;
			if (!(byteArray is null))
			{
				await res.OutputStream.WriteAsync(byteArray, 0, byteArray.Length);
				res.OutputStream.Close();
			}
			else
			{
				MakeEmptyResponse(res, HttpStatusCode.NoContent);
			}
		}

		public static void MakeEmptyResponse(HttpListenerResponse res, HttpStatusCode statusCode)
		{
			res.StatusCode = (int)statusCode;
			res.OutputStream.Close();
		}

		public async Task ListenAsync()
		{
			await TryStartAsync();
			await ListenTask;
		}

		public void On(string method, string relativePath, BoolCallback callback, bool not = false)
		{
			On(method, relativePath, (req, res) => Task.FromResult(callback(req, res)), not);
		}

		public void On(string method, string relativePath, VoidCallback callback, bool not = false)
		{
			On(method, relativePath, (req, res) => { callback(req, res); return Task.CompletedTask; }, not);
		}

		public void On(string method, string relativePath, AsyncBoolCallback callback, bool not = false)
		{
			callbacks[method].Add((Regex.Replace(relativePath, "\\?/", @"\/"), callback, not));
		}

		public void On(string method, string relativePath, AsyncCallback callback, bool not = false)
		{
			On(method, relativePath, async (req, res) => { await callback(req, res); return true; }, not);
		}

		protected override async Task PauseAsync()
		{
			cancellationTokenSource.Cancel();
			runServer = false;
			await AsyncManager.WaitForAnyCompletionAsync(ListenTask, cancellationTokenSource.Token);
			listener.Stop();
		}

		protected override async Task StopAsync()
		{
			await PauseAsync();
			listener.Close();
		}

		protected override Task StartAsync()
		{
			// Create start listening for incoming connections

			if (rootUri.Port == 0)
			{
				BindListenerOnFreePort();
			}
			else
			{
				listener.Prefixes.Add(RootUri.ToString());
				listener.Start();
			}

			Log($"Listening for connections on {RootUri}");

			// Handle requests
			ListenTask = HandleIncomingConnections();
			return Task.CompletedTask;
		}

		private void BindListenerOnFreePort()
		{
			// IANA suggested range for dynamic or private ports
			const int MinPort = 49215;
			const int MaxPort = 65535;

			Exception e1 = null;

			for (int port = MinPort; port < MaxPort; port++)
			{
				rootUri.Port = port;
				HttpListener httpListener = new HttpListener();

				try
				{
					httpListener.Prefixes.Add(RootUri.ToString());
					httpListener.Start();
					listener = httpListener;
					return;
				}
				catch (Exception e2)
				{
					Log(e2);
					e1 = e2;
				}
			}

			throw e1;
		}

		private async Task HandleIncomingConnections()
		{
			runServer = true;

			// Keep on handling requests until server is stopped
			while (runServer)
			{
				try
				{
					Log("Listening for a request");

					// Wait here until we hear from a connection
					HttpListenerContext ctx = await AsyncManager.Instance.MakeCancellable(listener.GetContextAsync(), cancellationTokenSource.Token);
					Log("Request received");
					if (!runServer)
					{
						break;
					}

					HttpListenerRequest request = ctx.Request;
					HttpListenerResponse response = ctx.Response;

					if (callbacks.TryGetValue(request.HttpMethod, out var dict))
					{
						foreach (var (pattern, callback, not) in dict)
						{
							if (Regex.IsMatch(request.Url.AbsolutePath, pattern) != not)
							{
								if (!await callback(request, response))
								{
									break;
								}
							}
						}
					}

					response.Close();
				}
				catch (Exception e) when (!(e is OperationCanceledException))
				{
					Debugger.Break();
				}
			}
		}
	}
}