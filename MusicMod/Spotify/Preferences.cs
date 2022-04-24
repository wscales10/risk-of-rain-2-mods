using System;
using System.IO;
using Utils;

namespace Spotify
{
	public static class Preferences
	{
		private static readonly string preferencesDirectory = Paths.AssemblyDirectory;

		public static event Action<string> PropertyChanged;

		public static byte[] DefaultDevice
		{
			get => ReadBytesFromFile(Path.Combine(preferencesDirectory, "defaultDevice.json"));

			set
			{
				WriteToFile(Path.Combine(preferencesDirectory, "defaultDevice.json"), value);
				PropertyChanged?.Invoke(nameof(DefaultDevice));
			}
		}

		internal static string AccessToken => ReadFromFile(
					Path.Combine(preferencesDirectory, "spotifyAccessToken.txt"),
			path => DateTime.UtcNow - File.GetLastWriteTimeUtc(path) < TimeSpan.FromHours(1))?.Trim();

		private static string ReadFromFile(string path, Predicate<string> predicate = null)
		{
			if (File.Exists(path) && (predicate is null || predicate(path)))
			{
				return File.ReadAllText(path);
			}
			else
			{
				return null;
			}
		}

		private static byte[] ReadBytesFromFile(string path, Predicate<string> predicate = null)
		{
			if (File.Exists(path) && (predicate is null || predicate(path)))
			{
				return File.ReadAllBytes(path);
			}
			else
			{
				return null;
			}
		}

		private static void WriteToFile(string path, byte[] contents)
		{
			if (contents is null)
			{
				File.Delete(path);
			}
			else
			{
				File.WriteAllBytes(path, contents);
			}
		}

		private static void WriteToFile(string path, string contents)
		{
			if (contents is null)
			{
				File.Delete(path);
			}
			else
			{
				File.WriteAllText(path, contents);
			}
		}
	}
}