using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace IPC.Http
{
	internal class HttpIpcClient : IAsyncSender
	{
		private readonly HttpClient httpClient = new HttpClient();

		private int receiverPort;

		public HttpIpcClient(string description)
		{
			Description = description;
		}

		public string Description { get; }

		public void Initialize(int receiverPort) => this.receiverPort = receiverPort;

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

			if (response.IsSuccessStatusCode)
			{
				var task2 = Task.Run(() => response.Content.ReadAsStringAsync());
				task2.Wait();
				return task2.Result;
			}
			else
			{
				throw new SendException();
			}
		}

		public async Task<string> SendAsync(string message)
		{
			var response = await PostMessageAsync(message);
			if (response.IsSuccessStatusCode)
			{
				return await response.Content.ReadAsStringAsync();
			}
			else
			{
				throw new SendException();
			}
		}

		private async Task<HttpResponseMessage> PostMessageAsync(string message)
		{
			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

			try
			{
				HttpResponseMessage httpResponseMessage = await httpClient.PostAsync($"http://localhost:{receiverPort}/", new StringContent(message), cancellationTokenSource.Token);
				return httpResponseMessage;
			}
			catch (SocketException ex)
			{
				throw new SendException(ex);
			}
			catch (HttpRequestException ex)
			{
				throw new SendException(ex);
			}
			catch (TaskCanceledException ex) when (ex.CancellationToken == cancellationTokenSource.Token)
			{
				throw new SendException(ex);
			}
		}
	}
}