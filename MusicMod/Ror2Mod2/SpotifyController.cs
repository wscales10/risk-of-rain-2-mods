using MyRoR2;
using Spotify;
using Spotify.Authorisation;
using Spotify.Commands;
using System;
using System.Threading.Tasks;
using Utils;

namespace Ror2Mod2
{
	public class SpotifyController : MusicBase
	{
		private readonly RulePicker rulePicker;

		private readonly ContextHelper contextHelper;

		public SpotifyController(RulePicker rulePicker, Logger logger) : base(logger)
		{
			this.rulePicker = rulePicker;
			Client = new SpotifyPlaybackClient(logger);
			Authorisation = new Authorisation(Scopes.Playback, logger: logger);
			Authorisation.OnAccessTokenReceived += Authorisation_OnAccessTokenReceived;
			Authorisation.OnClientRequested += Web.Goto;
			Authorisation.InitiateScopeRequest();
			contextHelper = new ContextHelper(Update, logger);
		}

		private void Authorisation_OnAccessTokenReceived(Authorisation sender, string accessToken)
		{
			Client.GiftNewAccessToken(accessToken);
		}

		protected SpotifyPlaybackClient Client { get; }

		public Authorisation Authorisation { get; }

		public override void Pause()
		{
			_ = Client.Pause();
		}

		public override void Resume()
		{
			_ = Client.Resume();
		}

		protected override async Task Play(object musicIdentifier)
		{
			if (!(musicIdentifier is null))
			{
				if (musicIdentifier is Command c)
				{
					await Client.Do(c);
				}
				else
				{
					throw new ArgumentException($"Expected a {nameof(Command)} but received a {musicIdentifier.GetType().Name} instead", nameof(musicIdentifier));
				}
			}
		}

		protected override object GetMusicIdentifier(Context oldContext, Context newContext)
		{
			return rulePicker.GetRule().GetCommands(oldContext, newContext);
		}

		protected override Context GetContext() => contextHelper.GetContext();
	}
}