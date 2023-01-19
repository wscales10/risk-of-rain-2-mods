using Microsoft.VisualStudio.Threading;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Utils;

namespace Music
{
	public abstract class MusicBase<TContext>
	{
		protected readonly Logger Log;

		private readonly AsyncSemaphore asyncSemaphore = new AsyncSemaphore(1);

		private TContext context;

		protected MusicBase(Logger logger = null)
		{
			Log = logger;
		}

		public abstract void OpenConfigurationPage();

		public abstract void Pause();

		public abstract void Resume();

		public async Task Update(TContext newContext)
		{
			using (await asyncSemaphore.EnterAsync())
			{
				var oldContext = context;
				await UpdateAsync(oldContext, newContext);
			}
		}

		protected abstract object GetMusicIdentifier(TContext oldContext, TContext newContext);

		protected abstract Task Play(object musicIdentifier);

		private async Task<bool> UpdateAsync(TContext oldContext, TContext newContext)
		{
			object musicIdentifier;

			try
			{
				musicIdentifier = GetMusicIdentifier(oldContext, newContext);
			}
			catch (Exception e)
			{
				Debugger.Break();
				return false;
			}

			await Play(musicIdentifier);
			context = newContext;
			return true;
		}
	}
}