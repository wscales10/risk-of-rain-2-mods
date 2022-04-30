using Patterns;
using Patterns.Patterns;
using System;
using WPFApp.Controls.GridManagers;
using WPFApp.Controls.Rows;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.PatternControls
{
	/// <summary>
	/// Interaction logic for ListPatternControl.xaml
	/// </summary>
	public partial class ListPatternControl : PatternControlBase, IRowControl
	{
		private readonly RowManager<PatternRow> rowManager;

		private readonly Func<SaveResult> tryGetValues;

		public ListPatternControl(IListPattern pattern, Type valueType, NavigationContext ctx) : base(ctx)
		{
			Item = pattern;
			InitializeComponent();
			NewPatternPicker patternPicker = new(valueType, ctx);
			patternPicker.SelectionMade += (w) => AddPatternWrapper(w);
			patternPickerContainer.Child = patternPicker;
			rowManager = new(patternsGrid);
			tryGetValues = rowManager.BindLooselyTo(Item.Children, (_) => throw new InvalidOperationException(), valuegetter);
			rowButtonsControl.BindTo(rowManager);
		}

		IRowManager IRowControl.RowManager => rowManager;

		public override IListPattern Item { get; }

		protected override SaveResult ShouldAllowExit() => tryGetValues();

		private static SaveResult<IPattern> valuegetter(PatternRow row)
		{
			var result = row.Output.TryGetObject(true);
			return SaveResult.Create<IPattern>(result);
		}

		private PatternRow AddPatternWrapper(IReadableControlWrapper patternWrapper) => rowManager.Add(new(patternWrapper));
	}
}