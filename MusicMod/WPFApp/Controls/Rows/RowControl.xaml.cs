using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPFApp.ViewModels;

namespace WPFApp.Controls.Rows
{
    /// <summary>
    /// Interaction logic for RowControl.xaml
    /// </summary>
    public partial class RowControl : UserControl
    {
        public RowControl() => InitializeComponent();

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register
        (
            nameof(Title),
            typeof(string),
            typeof(RowControl),
            new PropertyMetadata(null)
        );

        public object HelperContent
        {
            get => GetValue(HelperContentProperty);
            set => SetValue(HelperContentProperty, value);
        }

        public static readonly DependencyProperty HelperContentProperty = DependencyProperty.Register
        (
            nameof(HelperContent),
            typeof(object),
            typeof(RowControl),
            new PropertyMetadata(null)
        );

        public string NameWatermark
        {
            get => (string)GetValue(NameWatermarkProperty);
            set => SetValue(NameWatermarkProperty, value);
        }

        public static readonly DependencyProperty NameWatermarkProperty = DependencyProperty.Register
        (
            nameof(NameWatermark),
            typeof(string),
            typeof(RowControl),
            new PropertyMetadata(null)
        );

        public RowViewModelBase ViewModel => (RowViewModelBase)DataContext;

        private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var row = (IRow)((FrameworkElement)sender).DataContext;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.LeftShift))
            {
                ViewModel.JointSelect(row);
            }
            else
            {
                ViewModel.Select(row);
            }
        }
    }
}