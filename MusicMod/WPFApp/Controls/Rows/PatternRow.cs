using Patterns;
using System.Windows;
using System.Windows.Controls;
using WPFApp.Controls.Wrappers;
using WPFApp.Controls.Wrappers.PatternWrappers;

namespace WPFApp.Controls.Rows
{
	internal class PatternRow : Row<IReadableControlWrapper, PatternRow>
	{
		public PatternRow(IPattern pattern, IndexGetter<PatternRow> indexGetter, NavigationContext navigationContext)
			: this(PatternWrapper.Create(pattern, navigationContext), indexGetter)
		{
		}

		public PatternRow(IReadableControlWrapper patternWrapper, IndexGetter<PatternRow> indexGetter) : base(patternWrapper, indexGetter, true)
		{
		}

		public override bool TrySaveChanges() => Output.TryGetValue(out object _);

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