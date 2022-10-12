using Spotify.Authorisation;
using ZetaIpc.Runtime.Client;
using ZetaIpc.Runtime.Helper;
using ZetaIpc.Runtime.Server;

namespace SpotifyAuthenticationWinForms
{
	internal class MyServer
	{
		private static readonly string logFilePath = "log.txt";

		private readonly Dictionary<string, int> ports = new();

		private readonly Dictionary<string, IpcClient> clients = new();

		private string? lastToken;

		private Authorisation? authorisation;

		internal MyServer()
		{
			ClearLogFile();
			_ = DoServer();
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
			_ = authorisation.InitiateScopeRequestAsync();
			Form.RestartButtonEnabled = true;
		}

		private void OnExceptionOccurred(Exception exception)
		{
			LogInfo(exception.Message);
			throw exception;
		}

		private async Task DoServer()
		{
			var receiver = new IpcServer();
			receiver.Start(5007);

			receiver.ReceivedRequest += Receiver_ReceivedRequest;

			OnAccessTokenReceived += BroadcastToken;

			await Task.Delay(Timeout.InfiniteTimeSpan);

			void BroadcastToken(Authorisation _, string accessToken)
			{
				lastToken = accessToken;

				foreach (var client in clients.Values)
				{
					client.Send($"toke | {accessToken}");
				}
			}
		}

		private void Receiver_ReceivedRequest(object? sender, ReceivedRequestEventArgs e)
		{
			switch (e.Request[..4])
			{
				case "port":
					e.Response = $"port | {GetPort(e.Request[7..])}";
					break;

				case "conf":
					authorisation?.OpenConfigurationPage();
					break;

				case "conn":
					AddClient(e.Request[7..]);

					if (lastToken is not null)
					{
						e.Response = $"toke | {lastToken}";
					}
					break;
			}

			e.Handled = true;
		}

		private void AddClient(string guid)
		{
			var client = new IpcClient();
			clients[guid] = client;
			client.Initialize(ports[guid]);
		}

		private int GetPort(string guid)
		{
			int port = FreePortHelper.GetFreePort();
			ports[guid] = port;
			return port;
		}

		private void LogInfo(string message)
		{
			Utils.Logging.Log(this, message);
			File.AppendAllLines(logFilePath, new[] { message });
		}
	}
}