using Music;
using Spotify;
using Spotify.Commands;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Utils;
using Utils.Coroutines;

namespace SpotifyControlWinForms
{
	public class SpotifyController : MusicBase
	{
		private readonly AuthorisationClient authorisationClient = new();

		public SpotifyController(IEnumerable<Playlist> playlists, Logger logger) : base(logger)
		{
			Client = new SpotifyPlaybackClient(playlists, logger, authorisationClient.Preferences);
			Client.OnAccessTokenRequested += authorisationClient.RequestNewAccessToken;
			Client.OnError += e => Log(e);

			ConnectionUpdated += progressUpdate =>
			{
				switch (progressUpdate.Sender)
				{
					case IPC.Client _:
						switch (progressUpdate.Args)
						{
							case WebException ex:
								switch (ex.Status)
								{
									case WebExceptionStatus.Timeout:
										break;

									case WebExceptionStatus.ConnectFailure:
									case WebExceptionStatus.UnknownError:
										Thread.Sleep(1000);
										break;

									default:
										throw ex;
								}

								Log(ex);
								break;
						}
						break;

					default:
						throw new NotSupportedException(progressUpdate.Sender?.GetType().GetDisplayName());
				}

				return true;
			};

			ConfigurationPageRequested += () => authorisationClient.RequestConfigurationPage();
		}

		[SuppressMessage("Major Code Smell", "S3264:Events should be invoked", Justification = "Invoked Dynamically")]
		[SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "Necessary")]
		public event Func<ProgressUpdate, bool> ConnectionUpdated;

		private event Action ConfigurationPageRequested;

		protected SpotifyPlaybackClient Client { get; }

		public bool CheckStatus()
		{
			return authorisationClient.PingServer();
		}

		[SuppressMessage("Minor Code Smell", "S3267:Loops should be simplified with \"LINQ\" expressions", Justification = "To facilitate debugging")]
		public bool TryInit()
		{
			var run = authorisationClient.TryStart.CreateRun();

			foreach (var progressUpdate in run.GetProgressUpdates())
			{
				if (ConnectionUpdated is not null)
				{
					foreach (var connectionUpdateHandler in ConnectionUpdated.GetInvocationList().Cast<Func<ProgressUpdate, bool>>())
					{
						var result = connectionUpdateHandler(progressUpdate);

						if (!result)
						{
							run.Continue = false;
						}
					}
				}
			}

			return run.Result;
		}

		public override void Pause()
		{
			_ = Client.Pause();
		}

		public override void Resume()
		{
			_ = Client.Resume();
		}

		public override void OpenConfigurationPage()
		{
			ConfigurationPageRequested?.Invoke();
		}

		protected override async Task PlayAsync(object musicIdentifier)
		{
			if (musicIdentifier is not null)
			{
				switch (musicIdentifier)
				{
					case Command c:
						await Client.Do(c);
						break;

					case ICommandList commands:
						await Client.Do(commands);
						break;

					default:
						throw new ArgumentException($"Expected a {nameof(ICommandList)} but received a {musicIdentifier.GetType().Name} instead", nameof(musicIdentifier));
				}
			}
		}
	}
}