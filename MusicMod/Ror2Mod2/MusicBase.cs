using MyRoR2;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Utils;

namespace Ror2Mod2
{
	public abstract class MusicBase
	{
		private Context context;
		protected readonly Logger Log;

		protected MusicBase(Logger logger = null)
		{
			Log = logger;
		}

		public abstract void Pause();

		public abstract void Resume();

		protected abstract object GetMusicIdentifier(Context oldContext, Context newContext);

		protected abstract Task Play(object musicIdentifier);

		protected abstract Context GetContext();

		public async Task Update()
		{
			var oldContext = context;
			var newContext = GetContext();
            await UpdateAsync(oldContext, newContext);
        }

        private async Task<bool> UpdateAsync(Context oldContext, Context newContext)
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
