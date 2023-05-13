using System.Text.RegularExpressions;
using Utils;

namespace SpotifyControlWinForms.Connections
{
	public class OverwatchConnection : ConnectionBase
	{
		private readonly Func<ConditionalValue<string>> workshopLogFolderGetter;

		private readonly Action<string> notifyUser;

		private FileWatcher? fileWatcher;

		private FileSystemWatcher directoryWatcher;

		public OverwatchConnection(Func<ConditionalValue<string>> workshopLogFolderGetter, Action<string> notifyUser)
		{
			this.workshopLogFolderGetter = workshopLogFolderGetter;
			this.notifyUser = notifyUser;
		}

		public event Action<LogEntry>? Output;

		public FileInfo? LogFile => fileWatcher?.FileInfo;

		public override bool Ping()
		{
			directoryWatcher?.Dispose();

			if (tryConnect(out var logFile))
			{
				Output?.Invoke(new(TimeSpan.Zero, string.Empty));
				SetLogFile(logFile.FullName);
				return true;
			}

			return false;
		}

		protected override bool TryConnect_Inner()
		{
			if (tryConnect(out var logFile))
			{
				fileWatcher = new(logFile, true);
				fileWatcher.FileChanged += FileWatcher_FileChanged;
				return fileWatcher.TryStart();
			}

			return false;
		}

		private bool tryConnect(out FileInfo? logFile)
		{
			var attempt = workshopLogFolderGetter();

			if (!attempt.HasValue)
			{
				logFile = null;
				return false;
			}

			DirectoryInfo? workshopLogFolder = new(attempt.Value);

			if (!workshopLogFolder.Exists)
			{
				notifyUser($"The folder '{workshopLogFolder.FullName}' does not exist.");
				logFile = null;
				return false;
			}

			directoryWatcher = new FileSystemWatcher(workshopLogFolder.FullName);

			directoryWatcher.Created += OnNewLogFileCreated;
			directoryWatcher.Error += OnError;
			directoryWatcher.EnableRaisingEvents = true;

			logFile = workshopLogFolder.EnumerateFiles().OrderByDescending(f => f.LastWriteTimeUtc).FirstOrDefault();

			if (logFile is null)
			{
				notifyUser($"There are no log files in the folder '{workshopLogFolder.FullName}'.");
				return false;
			}

			return true;
		}

		private void OnError(object sender, ErrorEventArgs e)
		{
			System.Diagnostics.Debugger.Break();
		}

		private void OnNewLogFileCreated(object sender, FileSystemEventArgs e)
		{
			Output?.Invoke(new(TimeSpan.Zero, string.Empty));
			SetLogFile(e.FullPath);
		}

		private void SetLogFile(string filePath)
		{
			if (fileWatcher is not null)
			{
				fileWatcher.FileChanged -= FileWatcher_FileChanged;
				fileWatcher.TryStop();
			}

			fileWatcher = new(new(filePath), false);
			fileWatcher.FileChanged += FileWatcher_FileChanged;
			fileWatcher.TryStart();
		}

		private void FileWatcher_FileChanged(string? obj)
		{
			if (obj is null)
			{
				return;
			}

			var match = LogEntry.Regex.Match(obj);

			if (TimeSpan.TryParse(match.Groups["time"].Value, out var time))
			{
				Output?.Invoke(new(time, match.Groups["text"].Value));
			}
		}

		public struct LogEntry
		{
			public LogEntry(TimeSpan totalTimeElapsed, string text)
			{
				TotalTimeElapsed = totalTimeElapsed;
				Text = text;
			}

			public static Regex Regex { get; } = new(@"^\[(?'time'\d\d:\d\d:\d\d)\] (?'text'.*)$");

			public TimeSpan TotalTimeElapsed { get; }

			public string Text { get; }
		}
	}
}