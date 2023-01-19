using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace IPC.Http
{
	internal class HttpIpcClient : IAsyncClient
	{
		private readonly HttpClient httpClient = new HttpClient();

		private int serverPort;

		public void Initialize(int serverPort) => this.serverPort = serverPort;

		public string Send(string message)
		{
			var task = Task.Run(() => PostMessage(message));

			try
			{
				task.Wait();
			}
			catch (AggregateException ae)
			{
				throw ae.InnerExceptions[0];
			}

			var response = task.Result;
			var task2 = Task.Run(() => response.Content.ReadAsStringAsync());
			task2.Wait();
			return task2.Result;
		}

		public async Task<string> SendAsync(string message)
		{
			var response = await PostMessage(message);
			return await response.Content.ReadAsStringAsync();
		}

		private Task<HttpResponseMessage> PostMessage(string message)
		{
			try
			{
				return httpClient.PostAsync($"http://localhost:{serverPort}/", new StringContent(message));
			}
			catch (SocketException ex)
			{
				throw new SendException(ex);
			}
		}
	}
}