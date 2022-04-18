using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spotify.Authorisation;
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
			var auth = new Authorisation(Scopes.Metadata, true, logger: s => this.Log(s));
			auth.OnAccessTokenReceived += (_, a) => this.Log(a);
			auth.OnClientRequested += Web.Goto;
			auth.InitiateScopeRequest();
			await auth.Lifecycle.Task;
		}
	}
}
