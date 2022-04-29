using System;
using System.Collections.Generic;
using System.Linq;

namespace WPFApp.Modes
{
	public abstract class ModeBase
	{
		public virtual IEnumerable<ButtonContext> Buttons { get; } = new[] { ButtonContext.Import, ButtonContext.Export, ButtonContext.New };
	}

	public class MainMode : ModeBase
	{
		public override IEnumerable<ButtonContext> Buttons => new[] { ButtonContext.Back, ButtonContext.Forward, ButtonContext.Up, ButtonContext.Home }.Concat(base.Buttons);
	}

	public class BucketMode : ModeBase
	{
	}

	public class ButtonContext
	{
		public ButtonContext(string text, Action action = null)
		{
			Text = text;
			Command = new(_ => action());
		}

		public static ButtonContext Back { get; } = new("Back");

		public static ButtonContext Forward { get; } = new("Forward");

		public static ButtonContext Up { get; } = new("Up");

		public static ButtonContext Home { get; } = new("Home");

		public static ButtonContext Import { get; } = new("Import XML");

		public static ButtonContext Export { get; } = new("Export XML");

		public static ButtonContext New { get; } = new("New...");

		public string Text { get; }

		public ButtonCommand Command { get; }
	}
}