namespace SpotifyAuthenticationWinForms
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			ContextMenuStrip menu = new();
			ToolStripMenuItem showButton = new("Show Control Window");
			showButton.Click += ShowButton_Click;
			menu.Items.Add(showButton);
			menu.Items.Add(new ToolStripSeparator());
			ToolStripMenuItem exitButton = new("Exit");
			exitButton.Click += ExitButton_Click;
			menu.Items.Add(exitButton);
			notifyIcon.ContextMenuStrip = menu;
		}

		public event Action? OnRestart;

		public event Action? OnConfigure;

		public event Action? OnResetAccess;

		public event Action? OnResetRefresh;

		internal bool RestartButtonEnabled { get => RestartButton.Enabled; set => RestartButton.Enabled = value; }

		internal void Authorisation_ErrorStateChanged(string obj) => ErrorStateLabel.Text = obj;

		internal void Authorisation_FlowStateChanged(string obj) => FlowStateLabel.Text = obj;

		private void ExitButton_Click(object? sender, EventArgs e)
		{
			Close();
		}

		private void ShowButton_Click(object? sender, EventArgs e)
		{
			ShowForm();
		}

		private void RestartButton_Click(object sender, EventArgs e)
		{
			RestartButton.Enabled = false;
			OnRestart?.Invoke();
		}

		private void ConfigureButton_Click(object sender, EventArgs e)
		{
			OnConfigure?.Invoke();
		}

		private void ResetAccessButton_Click(object sender, EventArgs e)
		{
			OnResetAccess?.Invoke();
		}

		private void ResetRefreshButton_Click(object sender, EventArgs e)
		{
			OnResetRefresh?.Invoke();
		}

		private void Form1_Resize(object sender, EventArgs e)
		{
			if (WindowState == FormWindowState.Minimized)
			{
				Hide();
			}
		}

		private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e) => ShowForm();

		private void ShowForm()
		{
			Show();
			WindowState = FormWindowState.Normal;
		}
	}
}