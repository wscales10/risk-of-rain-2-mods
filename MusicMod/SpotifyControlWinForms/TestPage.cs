using Minecraft;
using SpotifyControlWinForms.Units;

namespace SpotifyControlWinForms
{
	public partial class TestPage : UserControl
	{
		public TestPage()
		{
			InitializeComponent();
		}

		public SpotifyControl? SpotifyControl { private get; set; }

		private void button1_Click(object sender, EventArgs e)
		{
			MinecraftCategoriser.Instance.Ingest(new MinecraftContext()
			{
				Bosses = new List<Mob>(),
				Screen = Screens.MainMenu
			});
		}
	}
}