using System.Windows;
using System.Windows.Controls;

namespace WPFApp.Controls
{
    /// <summary>
    /// Interaction logic for Row.xaml
    /// </summary>
    public partial class Row : UserControl
    {
        public static readonly DependencyProperty LeftElementProperty = DependencyProperty.Register(nameof(LeftElement), typeof(UIElement), typeof(Row));

        public static readonly DependencyProperty RightElementProperty = DependencyProperty.Register(nameof(RightElement), typeof(UIElement), typeof(Row));

        public Row() => InitializeComponent();

        public UIElement LeftElement
        {
            get => (UIElement)GetValue(LeftElementProperty);
            set => SetValue(LeftElementProperty, value);
        }

        public UIElement RightElement
        {
            get => (UIElement)GetValue(RightElementProperty);
            set => SetValue(RightElementProperty, value);
        }
    }
}