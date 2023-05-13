using SpotifyControlWinForms.Connections;
using Utils;

namespace SpotifyControlWinForms
{
	public partial class ConnectionControl : UserControl
	{
		private readonly ConnectionBase connection;

		public ConnectionControl(ConnectionBase connection)
		{
			InitializeComponent();
			this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
			connection.ConnectionAttempted += Connection_ConnectionAttempted;
			nameLabel.Text = connection.GetType().GetDisplayName();
		}

		private void Connection_ConnectionAttempted(ConnectionBase _, bool obj)
		{
			SetConnectionStatus(obj);
		}

		private void SetConnectionStatus(bool status)
		{
			if (status)
			{
				connectionButton.Text = "Connected";
				connectionButton.Enabled = false;
				pingButton.Enabled = true;
			}
			else
			{
				connectionButton.Text = "Retry Connection";
				connectionButton.Enabled = true;
				pingButton.Enabled = false;
			}
		}

		private void connectionButton_Click(object sender, EventArgs e)
		{
			connection.TryConnect();
		}

		private void pingButton_Click(object sender, EventArgs e)
		{
			SetConnectionStatus(connection.Ping());
		}
	}
}