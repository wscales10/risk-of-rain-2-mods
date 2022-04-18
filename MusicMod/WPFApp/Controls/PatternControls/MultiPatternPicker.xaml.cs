using System;
using System.Linq;
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

		internal bool TrySaveChanges() => PatternContainerManager.Rows.All(c => c.PatternWrapper.TryGetValue(out object _));

		protected override void Init() => InitializeComponent();

		protected override void HandleSelection(IReadableControlWrapper patternWrapper) => _ = AddPatternWrapper(patternWrapper);

		private PatternContainer AddPatternWrapper(IReadableControlWrapper patternWrapper) => new(patternWrapper, PatternContainerManager.Add);
	}
}