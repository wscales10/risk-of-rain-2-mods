using System;
using System.Windows.Controls.Primitives;
using WPFApp.Controls.GridManagers;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.PatternControls
{
	/// <summary>
	/// Interaction logic for MultiPatternPicker.xaml
	/// </summary>
	public partial class MultiPatternPicker : PatternPicker
	{
		public MultiPatternPicker(Type valueType, NavigationContext navigationContext) : base(valueType, navigationContext) => patternsControl.ItemsSource = PatternContainerManager.Items;

		public PatternContainerManager PatternContainerManager { get; } = new();

		protected override Selector ItemsControl => comboBox.ListBox;

		internal PatternContainer AddPatternWrapper(IReadableControlWrapper patternWrapper) => PatternContainerManager.Add(new(patternWrapper));

		protected override void Init() => InitializeComponent();

		protected override void handleSelection(IReadableControlWrapper patternWrapper) => _ = AddPatternWrapper(patternWrapper);
	}
}