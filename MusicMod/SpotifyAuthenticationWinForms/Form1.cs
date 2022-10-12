using H.Pipes;
using Spotify.Authorisation;
using System.Diagnostics;
using Utils;

namespace SpotifyAuthenticationWinForms
{
	public partial class Form1 : Form
	{
		private readonly Stack<string> logMessages = new();

		private Authorisation authorisation;

		private PipeServer<string> pipeServer;

		private string? lastToken;

		public Form1()
		{
			InitializeComponent();
			_ = DoServer();
			Setup();
		}

		private void ClientConnected(object? o, H.Pipes.Args.ConnectionEventArgs<string> args)
		{
			this.Log($"Client {args.Connection.PipeName} is now connected!");

			if (lastToken is not null)
			{
				_ = args.Connection.WriteAsync(lastToken);
			}
		}

		private void Authorisation_ErrorStateChanged(string obj) => ErrorStateLabel.Text = obj;

		private void Authorisation_FlowStateChanged(string obj) => FlowStateLabel.Text = obj;

		// private void Log(object obj) => logMessages.Push(obj?.ToString() ?? "[[null]]");

		private void RestartButton_Click(object sender, EventArgs e)
		{
			RestartButton.Enabled = false;

			_ = Restart();

			// Setup();
		}

		private async Task Restart()
		{
			try
			{
				this.Log("Stop Authorisation");
				await authorisation.TryStopAsync();
				this.Log("Start Authorisation");
				await authorisation.TryStartAsync();
				this.Log("Authorisation Restarted");
			}
			catch
			{
				Setup();
			}
		}

		private void Setup()
		{
			this.Log("New Authorisation");
			authorisation = new(Scopes.Playback, logger: x => this.Log(x));
			authorisation.OnClientRequested += Web.Goto;
			authorisation.OnAccessTokenReceived += BroadcastToken;
			authorisation.FlowStateChanged += Authorisation_FlowStateChanged;
			authorisation.ErrorStateChanged += Authorisation_ErrorStateChanged;
			_ = authorisation.InitiateScopeRequestAsync();
			RestartButton.Enabled = true;
		}

		private async Task DoServer()
		{
			if (pipeServer is not null)
			{
				throw new InvalidOperationException();
			}

			pipeServer = new PipeServer<string>("spotify-authorisation");
			pipeServer.ClientConnected += ClientConnected;

			pipeServer.ClientDisconnected += (o, args) =>
			{
				this.Log($"Client {args.Connection.PipeName} disconnected");
			};

			pipeServer.ExceptionOccurred += (o, args) => OnExceptionOccurred(args.Exception);

			await pipeServer.StartAsync();
			authorisation.OnAccessTokenReceived += BroadcastToken;

			await Task.Delay(Timeout.InfiniteTimeSpan);
		}

		private void BroadcastToken(Authorisation _, string accessToken) => pipeServer.WriteAsync(lastToken = accessToken);

		private void OnExceptionOccurred(Exception exception)
		{
			throw new NotImplementedException();
		}

		private void ConfigureButton_Click(object sender, EventArgs e)
		{
			authorisation.OpenConfigurationPage();
		}
	}
}