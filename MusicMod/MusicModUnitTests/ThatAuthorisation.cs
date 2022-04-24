using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spotify;
using Spotify.Authorisation;
using Spotify.Commands;
using System.Threading;
using System.Threading.Tasks;
using Utils;

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
			auth.InitiateScopeRequest();
			await auth.Lifecycle.Task;
		}

		[TestMethod]
		public async Task TestMethod1()
		{
			var auth = new Authorisation(Scopes.Playback, true, logger: s => this.Log(s));
			var client = new SpotifyPlaybackClient(s => this.Log(s));
			auth.OnAccessTokenReceived += (s, a) =>
			{
				client.GiftNewAccessToken(a);
				_ = client.Do(new PlayCommand(SpotifyItemType.Track, "2pl3Mzh2LeeUyzFacnHyZc"));
			};
			auth.OnClientRequested += Web.Goto;
			auth.InitiateScopeRequest();
			await auth.Lifecycle.Task;
		}
	}
}