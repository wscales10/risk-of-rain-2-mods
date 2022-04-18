using System;

namespace Spotify.Authorisation
{
	internal class App
	{
		public App(string clientSecret, string clientId, string baseUri, string redirectUri = null)
		{
			ClientSecret = clientSecret;
			ClientId = clientId;
			RootUri = new Uri(baseUri);
			this.redirectUri = redirectUri;
		}

		internal static App Instance = new App("25c6e6ffd8bf49edb4601428c715c3d3", "60f87e5bc07f48999e37f1581d9779c8", "http://localhost:5006/");
		private readonly string redirectUri;

		internal string ClientSecret { get; }

		internal string ClientId { get; }

		internal Uri RootUri { get; }

		internal Uri RedirectUri => redirectUri is null ? new Uri(RootUri, "callback") : new Uri(redirectUri);
	}
}
