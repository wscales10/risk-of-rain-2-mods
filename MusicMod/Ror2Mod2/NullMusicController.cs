using MyRoR2;
using System.Threading.Tasks;

namespace Ror2Mod2
{
	internal class NullMusicController : MusicBase
	{
		public override void Pause() { }

		protected override Task Play(object musicIdentifier) => Task.CompletedTask;

		public override void Resume() { }

		protected override object GetMusicIdentifier(Context oldContext, Context newContext) => null;

		protected override Context GetContext() => default;
	}
}
