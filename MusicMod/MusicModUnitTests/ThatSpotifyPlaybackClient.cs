using Microsoft.VisualStudio.TestTools.UnitTesting;
using Patterns.Patterns.SmallPatterns.ValuePatterns;
using Spotify;
using Spotify.Authorisation;
using Spotify.Commands;
using System.Linq;
using System.Threading.Tasks;
using Utils;

namespace MusicModUnitTests
{
    [TestClass]
    public class ThatSpotifyPlaybackClient
    {
        [TestMethod]
        public async Task TimesTransferCommandOptimally()
        {
            var auth = new Authorisation(Scopes.Playback, logger: s => this.Log(s));
            var client = new SpotifyPlaybackClient(Enumerable.Empty<Playlist>(), s => this.Log(s), -200);
            auth.OnAccessTokenReceived += (s, a) =>
            {
                client.GiftNewAccessToken(a);
                _ = Switch();
            };
            auth.OnClientRequested += Web.Goto;
            await auth.InitiateScopeRequestAsync();
            await auth.Info.WaitUntilOffAsync();

            async Task Switch()
            {
                await Task.Delay(2000);
                var music = new SpotifyItem(SpotifyItemType.Track, "4MuxsoMWAtAxeKo1r6Enj8");
                await client.Do(new PlayCommand(music));
                await Task.Delay(500);

                while (true)
                {
                    await client.Do(new TransferCommand(music, "ms", StringPattern.Equals("4MuxsoMWAtAxeKo1r6Enj8")));
                    await Task.Delay(2000);
                }
            }
        }
    }
}