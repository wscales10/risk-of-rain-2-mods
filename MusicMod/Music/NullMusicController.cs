using System.Threading.Tasks;

namespace Music
{
	internal class NullMusicController : MusicBase
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

		protected override Task PlayAsync(object musicIdentifier) => Task.CompletedTask;
	}
}