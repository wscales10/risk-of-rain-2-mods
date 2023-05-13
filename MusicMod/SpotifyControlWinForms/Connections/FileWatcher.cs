using Microsoft.VisualStudio.Threading;
using Utils.Runners;

namespace SpotifyControlWinForms.Connections
{
	public class FileWatcher : Runner
	{
		private readonly CancellationTokenSource cancellationTokenSource = new();

		private readonly bool readToEndFirst;

		public FileWatcher(FileInfo fileInfo, bool readToEndFirst)
		{
			FileInfo = fileInfo;
			this.readToEndFirst = readToEndFirst;
		}

		public event Action<string?>? FileChanged;

		public FileInfo FileInfo { get; }

		protected override void Start()
		{
			_ = RunAsync(cancellationTokenSource.Token);
		}

		protected override void Stop()
		{
			cancellationTokenSource.Cancel();
		}

		private async Task RunAsync(CancellationToken cancellationToken = default)
		{
			if (FileInfo is null)
			{
				throw new InvalidOperationException();
			}

			using FileStream fs = new(FileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			using StreamReader sr = new(fs);

			if (readToEndFirst)
			{
				await sr.ReadToEndAsync();
			}

			while (!cancellationToken.IsCancellationRequested)
			{
				while (!sr.EndOfStream)
				{
					FileChanged?.Invoke(await sr.ReadLineAsync());
				}

				while (sr.EndOfStream)
				{
					await Task.Delay(100, cancellationToken);
				}

				FileChanged?.Invoke(await sr.ReadLineAsync());
			}
		}
	}
}