using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace WPFApp.Controls.CommandControls
{
    /// <summary>
    /// Interaction logic for DiscreteComboBox.xaml
    /// </summary>
    public partial class DiscreteComboBox : UserControl
    {
        public static readonly DependencyProperty AlignmentProperty = DependencyProperty.Register(nameof(Alignment), typeof(HorizontalAlignment), typeof(DiscreteComboBox), new(HorizontalAlignment.Left));

        public DiscreteComboBox()
        {
            InitializeComponent();
            button.Click += Button_Click;
            LostFocus += DiscreteComboBox_LostFocus;
            ListBox.SelectionChanged += ListBox_SelectionChanged;
            popup.PlacementTarget = button;
            popup.Placement = PlacementMode.Bottom;
        }

        public DiscreteComboBox(HorizontalAlignment alignment, string displayMemberPath) : this()
        {
            Alignment = alignment;
            SetDisplayMemberPath(displayMemberPath);
        }

        internal event Action<object> OnSelectionMade;

        public HorizontalAlignment Alignment
        {
            get => (HorizontalAlignment)GetValue(AlignmentProperty);
            set => SetValue(AlignmentProperty, value);
        }

        public void SetDisplayMemberPath(string value) => ListBox.DisplayMemberPath = value;

        public void SetItemsSource(IEnumerable value) => ListBox.ItemsSource = value;

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => OnSelectionMade?.Invoke(((ListBox)sender).SelectedItem);

        private void DiscreteComboBox_LostFocus(object sender, RoutedEventArgs e) => popup.IsOpen = false;

        private void Button_Click(object sender, RoutedEventArgs e) => popup.IsOpen = true;
    }
}