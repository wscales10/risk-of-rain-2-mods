﻿using Rules;

namespace SpotifyControlWinForms
{
	public class Unit1<TIn, TOut> : Unit<TIn, TOut>
	{
		private readonly IRulePicker<TIn, TOut> rulePicker;

		public Unit1(IRulePicker<TIn, TOut> rulePicker)
		{
			this.rulePicker = rulePicker;
		}

		public override void Ingest(TIn input)
		{
			var output = rulePicker.Rule.GetOutput(input);
			Output(output);
		}
	}
}