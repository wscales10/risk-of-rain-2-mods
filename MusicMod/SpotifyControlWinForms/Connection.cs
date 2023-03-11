namespace SpotifyControlWinForms
{
	public class Connection : ConnectionBase
	{
		private readonly Func<bool> tryConnect;

		public Connection(Func<bool> tryConnect)
		{
			this.tryConnect = tryConnect;
		}

		public override bool Ping()
		{
			throw new NotImplementedException();
		}

		protected override bool TryConnect_Inner()
		{
			return tryConnect();
		}
	}
}