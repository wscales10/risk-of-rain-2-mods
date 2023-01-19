namespace SpotifyControlWinForms
{
	public abstract class Unit<TIn, TOut>
	{
		public event Action<TOut>? Trigger;

		public abstract void Ingest(TIn input);

		protected void Output(TOut output) => Trigger?.Invoke(output);
	}
}