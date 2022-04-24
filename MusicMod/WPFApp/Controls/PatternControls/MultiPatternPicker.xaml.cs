using System;
using System.Linq;
using System.Windows;
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
		public MultiPatternPicker(Type valueType, NavigationContext navigationContext) : base(valueType, navigationContext)
			=> PatternContainerManager = new(patternsGrid);

		public PatternContainerManager PatternContainerManager { get; }

		protected override Selector ItemsControl => comboBox.ListBox;

		internal bool TrySaveChanges() => PatternContainerManager.Items.All(c => c.PatternWrapper.TryGetValue(out object _));

		internal PatternContainer AddPatternWrapper(IReadableControlWrapper patternWrapper) => PatternContainerManager.Add(new(patternWrapper));

		protected override void Init() => InitializeComponent();

		protected override void handleSelection(IReadableControlWrapper patternWrapper) => _ = AddPatternWrapper(patternWrapper);
	}
}