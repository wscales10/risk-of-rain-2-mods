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

		public ListPatternControl(IListPattern pattern, Type valueType, NavigationContext ctx) : base(ctx)
		{
			Item = pattern;
			InitializeComponent();
			NewPatternPicker patternPicker = new(valueType, ctx);
			patternPicker.SelectionMade += (w) => AddPatternWrapper(w);
			patternPickerContainer.Child = patternPicker;
			rowManager = new(patternsGrid);
			rowManager.BindTo(Item.Children, AddPatternWrapper, r => r.Output);
		}

		public override IListPattern Item { get; }

		protected override bool ShouldAllowExit() => rowManager.TrySaveChanges();

		private PatternRow AddPatternWrapper(IReadableControlWrapper patternWrapper) => new(patternWrapper, rowManager.Add);
	}
}