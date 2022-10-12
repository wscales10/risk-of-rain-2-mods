using System;
using System.IO;
using System.Text;
using Utils;

namespace Spotify
{
	public interface IPreferences
	{
		event Action<string> PropertyChanged;

		byte[] DefaultDevice { get; set; }

		string DefaultDeviceString { get; set; }

		string AccessToken { get; }
	}

	public class PreferencesLite : IPreferences
	{
		private byte[] defaultDevice;

		private string accessToken;

		public event Action<string> PropertyChanged;

		public byte[] DefaultDevice
		{
			get => defaultDevice; set

			{
				defaultDevice = value;
				PropertyChanged?.Invoke(nameof(DefaultDevice));
			}
		}

		public string DefaultDeviceString
		{
			get => Encoding.UTF8.GetString(DefaultDevice);
			set => DefaultDevice = Encoding.UTF8.GetBytes(value);
		}

		public string AccessToken => accessToken;
	}

	public class Preferences : IPreferences
	{
		private static readonly string preferencesDirectory = Paths.AssemblyDirectory;

		public event Action<string> PropertyChanged;

		public byte[] DefaultDevice
		{
			get => ReadBytesFromFile(Path.Combine(preferencesDirectory, "defaultDevice.json"));

			set
			{
				WriteToFile(Path.Combine(preferencesDirectory, "defaultDevice.json"), value);
				PropertyChanged?.Invoke(nameof(DefaultDevice));
			}
		}

		public string DefaultDeviceString
		{
			get => ReadFromFile(Path.Combine(preferencesDirectory, "defaultDevice.json"));

			set
			{
				WriteToFile(Path.Combine(preferencesDirectory, "defaultDevice.json"), value);
				PropertyChanged?.Invoke(nameof(DefaultDevice));
			}
		}

		public string AccessToken => ReadFromFile(
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