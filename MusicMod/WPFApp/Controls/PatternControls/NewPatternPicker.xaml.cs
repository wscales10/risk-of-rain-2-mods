using System;
using System.Windows.Controls.Primitives;
using WPFApp.Controls.Wrappers;
using System.Windows.Controls;

namespace WPFApp.Controls.PatternControls
{
    /// <summary>
    /// Interaction logic for NewPicker.xaml
    /// </summary>
    public partial class NewPicker : Picker
    {
        public NewPicker(Type valueType, NavigationContext navigationContext) : base(valueType, navigationContext, false)
        {
        }

        public event Action<IReadableControlWrapper> SelectionMade;

        protected override Selector ItemsControl => newPatternTypeComboBox;

        protected override void Init() => InitializeComponent();

        protected override void handleSelection(IReadableControlWrapper patternWrapper) => SelectionMade?.Invoke(patternWrapper);

        private void AddPatternButton_Click(object sender, System.Windows.RoutedEventArgs e) => CommitSelection();

        private void newPatternTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => AddPatternButton.IsEnabled = e.AddedItems?.Count > 0;
    }
}