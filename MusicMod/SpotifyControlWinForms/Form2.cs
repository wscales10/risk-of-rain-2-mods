namespace SpotifyControlWinForms
{
	public partial class Form2 : Form
	{
		public Form2(SpotifyControl spotifyControl)
		{
			InitializeComponent();

			tabControl.TabPages.Remove(testPage);
			testPage1.SpotifyControl = spotifyControl;

			foreach (var connection in spotifyControl.Connections)
			{
				connectionsLayoutPanel.Controls.Add(new ConnectionControl(connection));
				connection.TryConnect();
			}

			foreach (var unit in spotifyControl.Units)
			{
				unitsLayoutPanel.Controls.Add(new UnitControl(unit));
			}
		}

		private void Form2_KeyPress(object sender, KeyPressEventArgs e)
		{
			switch (e.KeyChar)
			{
				case '`':
					if (tabControl.TabPages.Contains(testPage))
					{
						tabControl.TabPages.Remove(testPage);
					}
					else
					{
						tabControl.TabPages.Add(testPage);
					}

					break;
			}
		}
	}
}