using System.Collections.ObjectModel;
using Utils;

namespace Spotify.Authorisation
{
	public static class Scopes
	{
		public static ReadOnlyCollection<string> Playback = new[]
		{
			"user-modify-playback-state",
			"user-read-playback-position",
			"user-read-playback-state",
			"user-read-currently-playing"
		}.ToReadOnlyCollection();

		public static ReadOnlyCollection<string> Metadata = new[]
		{
			"playlist-read-private"
		}.ToReadOnlyCollection();
	}
}
