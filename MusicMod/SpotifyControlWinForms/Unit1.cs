using Rules;

namespace SpotifyControlWinForms
{
	public class Unit1<TIn, TOut> : Unit<TIn, TOut>
	{
		private readonly IRulePicker<TIn, TOut> rulePicker;

		private TIn? cached;

		public Unit1(IRulePicker<TIn, TOut> rulePicker)
		{
			this.rulePicker = rulePicker;
		}

		public override void Ingest(TIn input)
		{
			var output = rulePicker.Rule.GetOutput(cached, input);
			cached = input;
			Output(output);
		}
	}
}