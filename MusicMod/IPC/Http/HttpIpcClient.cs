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
			var task = Task.Run(() => PostMessageAsync(message));

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
			var response = await PostMessageAsync(message);
			return await response.Content.ReadAsStringAsync();
		}

		private async Task<HttpResponseMessage> PostMessageAsync(string message)
		{
			try
			{
				return await httpClient.PostAsync($"http://localhost:{serverPort}/", new StringContent(message));
			}
			catch (SocketException ex)
			{
				throw new SendException(ex);
			}
			catch (HttpRequestException ex)
			{
				throw new SendException(ex);
			}
		}
	}
}