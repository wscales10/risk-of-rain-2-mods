using Rules;
using Spotify;
using Spotify.Commands;
using System.Xml.Linq;
using Utils;
using Utils.Coroutines;

namespace SpotifyControlWinForms
{
	public class MusicConnection : ConnectionBase
	{
		private readonly List<Playlist> playlists = new();

		private MusicConnection()
		{
			Music = new(playlists, x => this.Log(x));
			Music.ConnectionUpdated += Music_ConnectionUpdateHandler;
		}

		public static MusicConnection Instance { get; } = new();

		public SpotifyController Music { get; private set; }

		public override bool Ping() => Music.CheckStatus();

		public void SetPlaylists(string? uri)
		{
			playlists.Clear();
			if (!string.IsNullOrEmpty(uri))
			{
				this.Log($"Playlists Location: {uri}");
				var playlistsFile = new FileInfo(uri).Directory?.GetFiles("playlists.xml").FirstOrDefault();
				if (playlistsFile is not null)
				{
					var imported = XElement.Load(playlistsFile.FullName).Elements().Select(x => new Playlist(x));
					if (imported is not null)
					{
						playlists.AddRange(imported);
					}
				}
			}
		}

		protected override bool TryConnect_Inner()
		{
			return Music.TryInit();
		}

		private bool Music_ConnectionUpdateHandler(ProgressUpdate arg)
		{
			if (arg.Args is Exception ex)
			{
				MessageBox.Show("Music connection error", ex.Message);
				return false;
			}

			return true;
		}
	}
}