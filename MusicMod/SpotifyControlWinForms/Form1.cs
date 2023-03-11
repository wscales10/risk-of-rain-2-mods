using SpotifyControlWinForms.Properties;

namespace SpotifyControlWinForms
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			/*			_ = new Tab(gameToStringTab, () => SpotifyControl.Rule1Type.GetLocation(), s =>
						{
							SpotifyControl.Rule1Type.SetLocation(s);
							Settings.Default.Save();
							Rule1LocationChanged?.Invoke(s);
						});

						_ = new Tab(stringToMusicTab, () => SpotifyControl.Rule2Type.GetLocation(), s =>
						{
							SpotifyControl.Rule2Type.SetLocation(s);
							Settings.Default.Save();
							Rule2LocationChanged?.Invoke(s);
						});*/

			Resize += Form1_Resize;
		}

		public event Action TryReconnect;

		public event Action<string> Rule1LocationChanged;

		public event Action<string> Rule2LocationChanged;

		private void Form1_Resize(object? sender, EventArgs e)
		{
			tabControl1.Width = Width - 40;
			tabControl1.Height = Height - 63;
		}

		private void ConnectionButton_Click(object sender, EventArgs e)
		{
			TryReconnect?.Invoke();
		}

		private sealed class Tab
		{
			private readonly Action<string> saveRuleLocation;

			public Tab(TabPage tabPage, Func<string?> getRuleLocation, Action<string> saveRuleLocation)
			{
				TabPage = tabPage;
				this.saveRuleLocation = saveRuleLocation;
				Initialise();
				LocationLabel.Text = getRuleLocation();
				BrowseButton.Click += BrowseButton_Click;
			}

			public TabPage TabPage { get; }

			public Label CaptionLabel { get; } = new() { Text = "Rule Location: ", Location = new(6, 3) };

			public Label LocationLabel { get; } = new() { AutoEllipsis = true, Location = new(97, 3) };

			public Button BrowseButton { get; } = new() { Text = "Browse...", Location = new(6, 27) };

			private void BrowseButton_Click(object? sender, EventArgs e)
			{
				var openFileDialog = new OpenFileDialog() { FileName = LocationLabel.Text, Filter = "XML Files (*.xml)|*.xml|All files (*.*)|*.*" };

				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					saveRuleLocation(LocationLabel.Text = openFileDialog.FileName);
				}
			}

			private void Initialise()
			{
				TabPage.Controls.Add(CaptionLabel);
				TabPage.Controls.Add(LocationLabel);
				TabPage.Controls.Add(BrowseButton);
			}
		}
	}
}