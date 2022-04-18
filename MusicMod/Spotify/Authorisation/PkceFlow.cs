using System.Collections.Generic;
using System.Collections.Specialized;
using SpotifyAPI.Web;

namespace Spotify.Authorisation
{
	internal class PkceFlow : CodeFlow
	{
		private readonly string codeVerifier;
		private readonly string codeChallenge;

		public PkceFlow(App app) : base(app)
		{
			(codeVerifier, codeChallenge) = PKCEUtil.GenerateCodes();
		}

		protected override Dictionary<string, string> GetAccessTokenRequest()
		{
			var output = base.GetAccessTokenRequest();
			output["client_id"] = app.ClientId;
			output["code_verifier"] = codeVerifier;
			return output;
		}

		public override NameValueCollection GetLoginQueryString(IEnumerable<string> scopes)
		{
			var output = base.GetLoginQueryString(scopes);
			output.Add("code_challenge_method", "S256");
			output.Add("code_challenge", codeChallenge);
			return output;
		}

		protected override Dictionary<string, string> GetRefreshTokenRequest()
		{
			var output = base.GetRefreshTokenRequest();
			output["client_id"] = app.ClientId;
			return output;
		}
	}
}
