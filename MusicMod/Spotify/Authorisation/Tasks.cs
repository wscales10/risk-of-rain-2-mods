using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Utils.Async;

namespace Spotify.Authorisation
{
	[SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "Tasks")]
	public partial class Authorisation
	{
		private readonly SingletonTask refresh;

		private readonly SingletonTask stop;

		public SingletonTaskWithSetup Lifecycle { get; }

		private async Task RefreshLoop()
		{
			while (true)
			{
				if (IsPaused)
				{
					await pauseEvent;
				}

				var delay = refreshBy - DateTime.UtcNow;

				if (delay > TimeSpan.Zero)
				{
					var delayTask = Task.Delay(delay, cancellationTokenSource.Token);
					await delayTask;

					if (delayTask.IsCanceled)
					{
						break;
					}
				}

				data = await flow.TryRefreshTokenAsync(client.SendAsync);

				if (!(data?.Scope is null))
				{
					foreach (var scope in data.Scope.Split(' '))
					{
						scopes[scope] = true;
					}
				}

				if (data is null)
				{
					flow = null;
					return;
				}

				OnAccessTokenReceived?.Invoke(this, data.AccessToken);
			}
		}

		private async Task StopInner()
		{
			cancellationTokenSource.Cancel();
			await server.StopAsync();
			await refresh.Task;
			stopEvent.Set();
		}

		private void StartInner()
		{
			stopEvent.Reset();
			flow = getFlow();
			_ = server.ListenAsync();
		}
	}
}