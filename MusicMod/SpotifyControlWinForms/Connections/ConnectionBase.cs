namespace SpotifyControlWinForms.Connections
{
	public delegate void ConnectionAttemptedEventHandler(ConnectionBase sender, bool result);

	public abstract class ConnectionBase
	{
		public event ConnectionAttemptedEventHandler? ConnectionAttempted;

		public void TryConnect()
		{
			ConnectionAttempted?.Invoke(this, TryConnect_Inner());
		}

		public abstract bool Ping();

		protected abstract bool TryConnect_Inner();
	}
}