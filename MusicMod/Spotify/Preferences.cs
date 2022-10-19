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
			get => DefaultDevice is null ? null : Encoding.UTF8.GetString(DefaultDevice);
			set => DefaultDevice = value is null ? null : Encoding.UTF8.GetBytes(value);
		}

		public string AccessToken
		{
			get => accessToken;

			set
			{
				accessToken = value;
				PropertyChanged?.Invoke(nameof(AccessToken));
			}
		}
	}

	public class Preferences : IPreferences
	{
		private static readonly string preferencesDirectory = Paths.AssemblyDirectory;

		public event Action<string> PropertyChanged;

		public byte[] DefaultDevice
		{
			get => ReadBytesFromFile(defaultDeviceFilePath);

			set
			{
				WriteToFile(defaultDeviceFilePath, value);
				PropertyChanged?.Invoke(nameof(DefaultDevice));
			}
		}

		public string DefaultDeviceString
		{
			get => ReadFromFile(defaultDeviceFilePath);

			set
			{
				WriteToFile(defaultDeviceFilePath, value);
				PropertyChanged?.Invoke(nameof(DefaultDevice));
			}
		}

		public string AccessToken
		{
			get
			{
				return ReadFromFile(
					Path.Combine(accessTokenFilePath),
					path => DateTime.UtcNow - File.GetLastWriteTimeUtc(path) < TimeSpan.FromHours(1))?.Trim();
			}

			set
			{
				WriteToFile(accessTokenFilePath, value);
				PropertyChanged?.Invoke(nameof(AccessToken));
			}
		}

		public string RefreshToken
		{
			get
			{
				return ReadFromFile(Path.Combine(refreshTokenFilePath))?.Trim();
			}

			set
			{
				WriteToFile(refreshTokenFilePath, value);
				PropertyChanged?.Invoke(nameof(RefreshToken));
			}
		}

		public DateTime AccessTokenUpdated => File.GetLastWriteTimeUtc(accessTokenFilePath);

		private string refreshTokenFilePath => Path.Combine(preferencesDirectory, "spotifyRefreshToken.txt");

		private string accessTokenFilePath => Path.Combine(preferencesDirectory, "spotifyAccessToken.txt");

		private string defaultDeviceFilePath => Path.Combine(preferencesDirectory, "defaultDevice.json");

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