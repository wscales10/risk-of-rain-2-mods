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
	public partial class ListPatternControl : PatternControlBase
	{
		private readonly RowManager<PatternRow> rowManager;

		private readonly Func<bool> tryGetValues;

		public ListPatternControl(IListPattern pattern, Type valueType, NavigationContext ctx) : base(ctx)
		{
			Item = pattern;
			InitializeComponent();
			NewPatternPicker patternPicker = new(valueType, ctx);
			patternPicker.SelectionMade += (w) => AddPatternWrapper(w);
			patternPickerContainer.Child = patternPicker;
			rowManager = new(patternsGrid);
			tryGetValues = rowManager.BindLooselyTo<IPattern>(Item.Children, (_) => throw new InvalidOperationException(), valuegetter);
			rowButtonsControl.BindTo(rowManager);
		}

		public override IListPattern Item { get; }

		protected override bool ShouldAllowExit() => tryGetValues();

		private static bool valuegetter(PatternRow row, out IPattern value)
		{
			bool result = row.Output.TryGetValue(out object obj);
			value = (IPattern)obj;
			return result;
		}

		private PatternRow AddPatternWrapper(IReadableControlWrapper patternWrapper) => rowManager.Add(new(patternWrapper));
	}
}