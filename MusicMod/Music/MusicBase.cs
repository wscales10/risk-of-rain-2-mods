using Microsoft.VisualStudio.Threading;
using System.Threading.Tasks;
using Utils;

namespace Music
{
	public abstract class MusicBase
	{
		protected readonly Logger Log;

		private readonly AsyncSemaphore asyncSemaphore = new AsyncSemaphore(1);

		protected MusicBase(Logger logger = null)
		{
			Log = logger;
		}

		public abstract void OpenConfigurationPage();

		public abstract void Pause();

		public abstract void Resume();

		public void Play(object musicIdentifier)
		{
			_ = PlayAsyncWrapper(musicIdentifier);
		}

		protected abstract Task PlayAsync(object musicIdentifier);

		private async Task PlayAsyncWrapper(object musicIdentifier)
		{
			using (await asyncSemaphore.EnterAsync())
			{
				await PlayAsync(musicIdentifier);
			}
		}
	}
}