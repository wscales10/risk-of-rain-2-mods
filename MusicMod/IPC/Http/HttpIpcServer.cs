using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace IPC.Http
{
	internal class HttpIpcServer : IServer
	{
		private HttpServer httpServer;

		public HttpIpcServer()
		{
		}

		public event Func<string, string> ReceivedRequest;

		public void Start(int port)
		{
			httpServer = new HttpServer(new Uri($"http://localhost:{port}/"), x => this.Log(x));
			httpServer.On("POST", "", HttpServer_ReceivedRequest);
			var task = Task.Run(() => httpServer.TryStartAsync());
			task.Wait();
		}

		private async Task HttpServer_ReceivedRequest(HttpListenerRequest request, HttpListenerResponse response)
		{
			var requestMessage = new StreamReader(request.InputStream).ReadToEnd();
			var responseMessage = ReceivedRequest?.Invoke(requestMessage);
			response.ContentType = "text/plain";
			await HttpServer.MakeResponseAsync(response, Encoding.UTF8.GetBytes(responseMessage));
		}
	}
}