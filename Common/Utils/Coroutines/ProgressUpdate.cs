namespace Utils.Coroutines
{
	public class ProgressUpdate
	{
		public ProgressUpdate(object sender, object args)
		{
			Sender = sender;
			Args = args;
		}

		public object Sender { get; }

		public object Args { get; }
	}
}