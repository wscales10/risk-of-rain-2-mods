using Music;
using Rules;
using Spotify;
using Spotify.Commands;
using System.Net;
using Utils;
using Utils.Coroutines;

namespace SpotifyControlWinForms
{
	public class SpotifyController<TContext> : MusicBase<TContext>
	{
		private readonly AuthorisationClient authorisationClient = new AuthorisationClient();

		private readonly IRulePicker<TContext, ICommandList> rulePicker;

		public SpotifyController(IRulePicker<TContext, ICommandList> rulePicker, IEnumerable<Playlist> playlists, Logger logger) : base(logger)
		{
			this.rulePicker = rulePicker;
			Client = new SpotifyPlaybackClient(playlists, logger, authorisationClient.Preferences);
			Client.OnAccessTokenRequested += authorisationClient.RequestNewAccessToken;
			Client.OnError += e => Log(e);

			ConnectionUpdateHandler += progressUpdate =>
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

		public event Func<ProgressUpdate, bool> ConnectionUpdateHandler;

		private event Action ConfigurationPageRequested;

		protected SpotifyPlaybackClient Client { get; }

		public bool TryInit()
		{
			var run = authorisationClient.TryStart.CreateRun();

			foreach (var progressUpdate in run.GetProgressUpdates())
			{
				if (!(ConnectionUpdateHandler is null))
				{
					foreach (var connectionUpdateHandler in ConnectionUpdateHandler.GetInvocationList())
					{
						var result = (bool)connectionUpdateHandler.DynamicInvoke(progressUpdate);

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

		protected override async Task Play(object musicIdentifier)
		{
			if (!(musicIdentifier is null))
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

		protected override object GetMusicIdentifier(TContext oldContext, TContext newContext)
		{
			if (newContext?.Equals(default) ?? true)
			{
				return new StopCommand();
			}

			var commands = rulePicker.Rule.GetOutput(newContext);
			return commands;
		}
	}
}