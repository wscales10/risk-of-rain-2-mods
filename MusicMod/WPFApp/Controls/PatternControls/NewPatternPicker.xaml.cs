using System;
using System.Windows.Controls.Primitives;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.PatternControls
{
	/// <summary>
	/// Interaction logic for NewPatternPicker.xaml
	/// </summary>
	public partial class NewPatternPicker : PatternPicker
	{
		protected override Selector ItemsControl => newPatternTypeComboBox;

		public NewPatternPicker(Type valueType, NavigationContext navigationContext) : base(valueType, navigationContext, false)
		{
		}

		public event Action<IReadableControlWrapper> SelectionMade;

		protected override void Init() => InitializeComponent();

		protected override void HandleSelection(IReadableControlWrapper patternWrapper) => SelectionMade?.Invoke(patternWrapper);

		private void AddPatternButton_Click(object sender, System.Windows.RoutedEventArgs e) => CommitSelection();
	}
}