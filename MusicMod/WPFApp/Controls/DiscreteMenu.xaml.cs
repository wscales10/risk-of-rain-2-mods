using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFApp.Controls
{
    /// <summary>
    /// Interaction logic for DiscreteMenu.xaml
    /// </summary>
    public partial class DiscreteMenu : Menu
    {
        public static readonly DependencyProperty AlignmentProperty = DependencyProperty.Register(nameof(Alignment), typeof(HorizontalAlignment), typeof(DiscreteMenu), new(HorizontalAlignment.Left));

        public DiscreteMenu()
        {
            InitializeComponent();
            button.Click += Button_Click;
            LostFocus += DiscreteComboBox_LostFocus;
            ListBox.SelectionChanged += ListBox_SelectionChanged;
            popup.PlacementTarget = button;
            popup.Placement = PlacementMode.Bottom;
        }

        public DiscreteMenu(HorizontalAlignment alignment, string displayMemberPath) : this()
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