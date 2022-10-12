using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;

namespace Spotify.Authorisation
{
	[SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "Tasks")]
	public partial class Authorisation
	{
		protected override async Task StartAsync()
		{
			flow = getFlow();
			flow.StateChanged += Flow_StateChanged;
			flow.ErrorStateChanged += Flow_ErrorStateChanged;
			try
			{
				_ = await server.TryStartAsync();
			}
			catch (HttpListenerException e)
			{
				Debugger.Break();
			}
		}

		protected override async Task ResumeAsync()
		{
			_ = await server.TryResumeAsync();

			if (flow?.State >= FlowState.TokenGranted)
			{
				refreshTask = Async.Manager.RunSafely(RefreshLoop);
			}
		}

		private void Flow_ErrorStateChanged(ErrorState obj) => ErrorStateChanged?.Invoke(obj.ToString());

		private void Flow_StateChanged(FlowState obj) => FlowStateChanged?.Invoke(obj.ToString());

		private async Task RefreshLoop()
		{
			while (true)
			{
				var delay = RefreshBy - DateTime.UtcNow;

				if (delay > TimeSpan.Zero)
				{
					await Task.Delay(delay, cancellationTokenSource.Token);
				}

				data = await flow.TryRefreshTokenAsync();

				if (!(data?.Scope is null))
				{
					foreach (var scope in data.Scope.Split(' '))
					{
						scopes[scope] = true;
					}
				}

				switch (flow.State)
				{
					case FlowState.Error:
						switch (flow.ErrorState)
						{
							case ErrorState.Unlucky:
								RefreshIn(TimeSpan.FromSeconds(6));
								break;

							default:
								throw new NotImplementedException();
						}
						break;

					case FlowState.TokenRefreshed:

						if (data is null)
						{
							throw new InvalidOperationException();
						}

						OnAccessTokenReceived?.Invoke(this, data.AccessToken);
						RefreshIn(GetWaitTime(data.ExpiresIn));
						break;

					default:
						throw new NotImplementedException();
				}
			}
		}

		private void RefreshIn(TimeSpan timeSpan) => RefreshBy = DateTime.UtcNow + timeSpan;
	}
}