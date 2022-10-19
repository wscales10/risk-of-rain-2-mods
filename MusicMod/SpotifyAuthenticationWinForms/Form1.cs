namespace SpotifyAuthenticationWinForms
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		public event Action? OnRestart;

		public event Action? OnConfigure;

		public event Action? OnResetAccess;

		public event Action? OnResetRefresh;

		internal bool RestartButtonEnabled { get => RestartButton.Enabled; set => RestartButton.Enabled = value; }

		internal void Authorisation_ErrorStateChanged(string obj) => ErrorStateLabel.Text = obj;

		internal void Authorisation_FlowStateChanged(string obj) => FlowStateLabel.Text = obj;

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
	}
}