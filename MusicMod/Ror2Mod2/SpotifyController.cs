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
            Client.OnError += e => this.Log(e);
            Authorisation = new Authorisation(Scopes.Playback, logger: logger);
            Authorisation.OnAccessTokenReceived += Authorisation_OnAccessTokenReceived;
            Authorisation.OnClientRequested += Web.Goto;
            Authorisation.InitiateScopeRequest();
            contextHelper = new ContextHelper(Update, logger);
        }

        public Authorisation Authorisation { get; }

        protected SpotifyPlaybackClient Client { get; }

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

        protected override object GetMusicIdentifier(Context oldContext, Context newContext)
        {
            var commands = rulePicker.GetRule().GetCommands(oldContext, newContext);
            return commands;
        }

        protected override Context GetContext() => contextHelper.GetContext();

        private void Authorisation_OnAccessTokenReceived(Authorisation sender, string accessToken)
        {
            Client.GiftNewAccessToken(accessToken);
        }
    }
}