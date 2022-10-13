using Spotify;
using Spotify.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utils;

namespace Ror2Mod2
{
	public class SpotifyController<TContext> : MusicBase<TContext>
	{
		private readonly AuthorisationClient authorisationClient = new AuthorisationClient();

		private readonly IRulePicker<TContext, ICommandList> rulePicker;

		private readonly IContextHelper<TContext> contextHelper;

		public SpotifyController(IRulePicker<TContext, ICommandList> rulePicker, IEnumerable<Playlist> playlists, IContextHelper<TContext> contextHelper, Logger logger) : base(logger)
		{
			this.rulePicker = rulePicker;
			Client = new SpotifyPlaybackClient(playlists, logger, authorisationClient.Preferences);
			authorisationClient.OnNewAccessToken += Client.GiftNewAccessToken;
			Client.OnError += e => Log(e);

			authorisationClient.TryStart();
			ConfigurationPageRequested += () => authorisationClient.RequestConfigurationPage();
			this.contextHelper = contextHelper;
			contextHelper.NewContext += c => _ = Update(c);
		}

		private event Action ConfigurationPageRequested;

		protected SpotifyPlaybackClient Client { get; }

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
			var commands = rulePicker.Rule.GetCommands(oldContext, newContext);
			return commands;
		}
	}
}