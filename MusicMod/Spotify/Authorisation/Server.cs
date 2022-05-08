using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using Utils.Async;

namespace Spotify.Authorisation
{
    public class Server
    {
        private readonly Dictionary<string, List<(string, AsyncBoolCallback, bool)>> callbacks = new Dictionary<string, List<(string, AsyncBoolCallback, bool)>>();

        private readonly HttpListener listener = new HttpListener();

        private readonly Logger Log;

        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private bool runServer;

        public Server(Uri rootUri, Logger logger)
        {
            Listen = new SingletonTask(HandleIncomingConnections);
            listener.Prefixes.Add((RootUri = rootUri).ToString());
            Log = logger;
            callbacks["GET"] = new List<(string, AsyncBoolCallback, bool)>();
            callbacks["POST"] = new List<(string, AsyncBoolCallback, bool)>();
        }

        public SingletonTask Listen { get; }

        public Uri RootUri { get; }

        public async Task ListenAsync()
        {
            // Create start listening for incoming connections
            listener.Start();
            Log($"Listening for connections on {RootUri}");

            // Handle requests
            await Listen.RunAsync();
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

        public async Task PauseAsync()
        {
            cancellationTokenSource.Cancel();
            runServer = false;
            await Listen.Task;
            listener.Stop();
        }

        public async Task StopAsync()
        {
            await PauseAsync();
            listener.Close();
        }

        private async Task HandleIncomingConnections()
        {
            runServer = true;

            // Keep on handling requests until server is stopped
            while (runServer)
            {
                // Wait here until we hear from a connection
                var task = Async.Manager.MakeCancellable(listener.GetContextAsync(), cancellationTokenSource.Token);
                HttpListenerContext ctx = await task;

                if (task.IsCanceled || !runServer)
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
        }
    }
}