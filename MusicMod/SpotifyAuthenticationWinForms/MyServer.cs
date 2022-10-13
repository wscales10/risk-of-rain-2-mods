using IPC;
using Spotify.Authorisation;

namespace SpotifyAuthenticationWinForms
{
	internal class MyServer
	{
		private static readonly string logFilePath = "log.txt";

		private readonly IPC.Server server = new(5007);

		private string? lastToken;

		private Authorisation? authorisation;

		internal MyServer()
		{
			ClearLogFile();
			server.ReceivedRequest += Server_ReceivedRequest;
			server.TryStart();
			OnAccessTokenReceived += BroadcastToken;
			void BroadcastToken(Authorisation _, string accessToken)
			{
				lastToken = accessToken;
				server.Broadcast($"toke | {accessToken}");
			}

			Form = new();
			Form.OnRestart += () => _ = Restart();
			Form.OnConfigure += () => authorisation?.OpenConfigurationPage();
			SetupAuthorisation();
		}

		private event Authorisation.AccessTokenHandler? OnAccessTokenReceived;

		public Form1 Form { get; }

		private static void ClearLogFile() => File.Create(logFilePath).Close();

		private async Task Restart()
		{
			try
			{
				if (authorisation is null)
				{
					throw new InvalidOperationException($"{nameof(authorisation)} is null");
				}

				LogInfo("Stop Authorisation");
				await authorisation.TryStopAsync();
				LogInfo("Start Authorisation");
				await authorisation.TryStartAsync();
				LogInfo("Authorisation Restarted");
			}
			catch
			{
				SetupAuthorisation();
			}
		}

		private void SetupAuthorisation()
		{
			LogInfo("New Authorisation");
			authorisation = new(Scopes.Playback, logger: x => LogInfo(x?.ToString() ?? "[[null]]"));
			authorisation.OnClientRequested += Utils.Web.Goto;
			authorisation.OnAccessTokenReceived += (a, s) => OnAccessTokenReceived?.Invoke(a, s);
			authorisation.FlowStateChanged += Form.Authorisation_FlowStateChanged;
			authorisation.ErrorStateChanged += Form.Authorisation_ErrorStateChanged;
			authorisation.Preferences.PropertyChanged += (name) =>
			{
				if (name == nameof(authorisation.Preferences.DefaultDevice))
				{
					server.Broadcast($"devi | {authorisation.Preferences.DefaultDeviceString}");
				}
			};
			_ = authorisation.InitiateScopeRequestAsync();
			Form.RestartButtonEnabled = true;
		}

		private void OnExceptionOccurred(Exception exception)
		{
			LogInfo(exception.Message);
			throw exception;
		}

		private IEnumerable<string> Server_ReceivedRequest(IEnumerable<string> request)
		{
			foreach (var requestPart in request)
			{
				switch (requestPart[..4])
				{
					case "port":
						yield return $"port | {server.GetPort(requestPart[7..])}";
						break;

					case "conf":
						authorisation?.OpenConfigurationPage();
						break;

					case "conn":
						var client = server.AddClient(requestPart[7..]);

						if (authorisation is not null)
						{
							authorisation.Preferences.PropertyChanged += (name) =>
							{
								if (name == nameof(authorisation.Preferences.DefaultDevice))
								{
									Methods.SendMessage(client, $"devi | {authorisation.Preferences.DefaultDeviceString}");
								}
							};
						}

						if (lastToken is not null)
						{
							yield return $"toke | {lastToken}";
						}

						if (!string.IsNullOrWhiteSpace(authorisation?.Preferences.DefaultDeviceString))
						{
							yield return $"devi | {authorisation.Preferences.DefaultDeviceString}";
						}

						break;
				}
			}
		}

		private void LogInfo(string message)
		{
			Utils.Logging.Log(this, message);
			File.AppendAllLines(logFilePath, new[] { message });
		}
	}
}