using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace IPC.Http
{
	internal class HttpIpcServer : IReceiver
	{
		private HttpServer httpServer;

		public HttpIpcServer()
		{
		}

		public event Func<string, string> ReceivedRequest;

		public int? Port => httpServer?.RootUri.Port;

		public void Start(int port)
		{
			httpServer = new HttpServer(new Uri($"http://localhost:{port}/"), x => this.Log(x));
			httpServer.On("POST", "", HttpServer_ReceivedRequest);
			var task = Task.Run(() => httpServer.TryStartAsync());
			task.Wait();
		}

		[SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "Exception viewing")]
		private async Task HttpServer_ReceivedRequest(HttpListenerRequest request, HttpListenerResponse response)
		{
			var requestMessage = new StreamReader(request.InputStream).ReadToEnd();
			string responseMessage;
			try
			{
				responseMessage = ReceivedRequest?.Invoke(requestMessage);
			}
			catch (DeliveryException)
			{
				HttpServer.MakeEmptyResponse(response, HttpStatusCode.Gone);
				return;
			}
			catch (Exception ex)
			{
				// Return appropriate response
				System.Diagnostics.Debugger.Break();
				throw;
			}

			response.ContentType = "text/plain";
			await HttpServer.MakeResponseAsync(response, responseMessage);
		}
	}
}