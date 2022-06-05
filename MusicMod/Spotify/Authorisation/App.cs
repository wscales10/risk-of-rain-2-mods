using System;

namespace Spotify.Authorisation
{
    internal class App
    {
        internal static App Instance = new App("60f87e5bc07f48999e37f1581d9779c8", "http://localhost:5006/");

        private readonly string redirectUri;

        public App(string clientId, string baseUri, string redirectUri = null)
        {
            ClientId = clientId;
            RootUri = new Uri(baseUri);
            this.redirectUri = redirectUri;
        }

        internal string ClientId { get; }

        internal Uri RootUri { get; }

        internal Uri RedirectUri => redirectUri is null ? new Uri(RootUri, "callback") : new Uri(redirectUri);
    }
}