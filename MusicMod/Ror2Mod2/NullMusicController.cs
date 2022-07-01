using MyRoR2;
using System.Threading.Tasks;

namespace Ror2Mod2
{
    internal class NullMusicController<TContext> : MusicBase<TContext>
    {
        public override void Pause()
        {
        }

        public override void Resume()
        {
        }

        public override void OpenConfigurationPage()
        {
        }

        protected override Task Play(object musicIdentifier) => Task.CompletedTask;

        protected override object GetMusicIdentifier(TContext oldContext, TContext newContext) => null;
    }
}