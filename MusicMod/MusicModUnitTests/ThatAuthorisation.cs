using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spotify;
using Spotify.Authorisation;
using Spotify.Commands.Mutable;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using System;
using System.Linq;

namespace MusicModUnitTests
{
    [TestClass]
    public class ThatAuthorisation
    {
        [TestMethod]
        public async Task TestMethod6()
        {
            var auth = new Authorisation(Scopes.Playback, true, logger: s => this.Log(s));
            auth.OnAccessTokenReceived += (_, a) => this.Log(a);
            auth.OnClientRequested += Web.Goto;
            _ = auth.InitiateScopeRequestAsync();
            await auth.Info.WaitUntilOffAsync();
        }

        [TestMethod]
        public async Task TestMethod1()
        {
            var auth = new Authorisation(Scopes.Playback, true, logger: s => this.Log(s));
            var client = new SpotifyPlaybackClient(Enumerable.Empty<Playlist>(), s => this.Log(s));
            auth.OnAccessTokenReceived += (s, a) =>
            {
                client.GiftNewAccessToken(a);
                _ = client.Do(new PlayCommand(SpotifyItemType.Track, "2pl3Mzh2LeeUyzFacnHyZc"));
            };
            auth.OnClientRequested += Web.Goto;
            _ = auth.InitiateScopeRequestAsync();
            await auth.Info.WaitUntilOffAsync();
        }

        [TestMethod]
        public async Task ThatRefreshWorks()
        {
            var auth = new Authorisation(Scopes.Playback, logger: s => this.Log(s)) { GetWaitTime = _ => TimeSpan.FromSeconds(10) };
            auth.OnClientRequested += Web.Goto;
            _ = auth.InitiateScopeRequestAsync();
            await auth.Info.WaitUntilOffAsync();
        }
    }
}