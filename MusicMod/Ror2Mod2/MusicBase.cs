using Microsoft.VisualStudio.Threading;
using MyRoR2;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Utils;

namespace Ror2Mod2
{
    public abstract class MusicBase
    {
        protected readonly Logger Log;

        private readonly AsyncSemaphore asyncSemaphore = new AsyncSemaphore(1);

        private Context context;

        protected MusicBase(Logger logger = null)
        {
            Log = logger;
        }

        public abstract void Pause();

        public abstract void Resume();

        public async Task Update()
        {
            using (await asyncSemaphore.EnterAsync())
            {
                var oldContext = context;
                var newContext = GetContext();
                await UpdateAsync(oldContext, newContext);
            }
        }

        protected abstract object GetMusicIdentifier(Context oldContext, Context newContext);

        protected abstract Task Play(object musicIdentifier);

        protected abstract Context GetContext();

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