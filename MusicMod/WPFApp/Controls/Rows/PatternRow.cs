using Patterns;
using System.Windows;
using System.Windows.Controls;
using WPFApp.Controls.Wrappers;
using WPFApp.Controls.Wrappers.PatternWrappers;

namespace WPFApp.Controls.Rows
{
	internal class PatternRow : Row<IReadableControlWrapper, PatternRow>
	{
		public PatternRow(IPattern pattern, NavigationContext navigationContext)
			: this(PatternWrapper.Create(pattern, navigationContext))
		{
		}

		public PatternRow(IReadableControlWrapper patternWrapper) : base(patternWrapper, true)
		{
		}

		public override SaveResult TrySaveChanges() => Output.TryGetObject(true);

		protected override UIElement MakeOutputUi() => new Border()
		{
			Margin = new Thickness(40, 4, 4, 4),
			VerticalAlignment = VerticalAlignment.Center,
			MinWidth = 150,
			HorizontalAlignment = HorizontalAlignment.Left,
			Child = Output?.UIElement
		};
	}
}