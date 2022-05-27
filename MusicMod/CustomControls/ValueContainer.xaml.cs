using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace CustomControls
{
    /// <summary>
    /// Interaction logic for ValueContainer.xaml
    /// </summary>
    [ContentProperty(nameof(ContentUI))]
    public partial class ValueContainer : UserControl
    {
        public static readonly DependencyProperty ContentUIProperty = DependencyProperty.Register(nameof(ContentUI), typeof(object), typeof(ValueContainer));

        public ValueContainer()
        {
            InitializeComponent();
        }

        public event Action? Deleted;

        public object ContentUI
        {
            get => GetValue(ContentUIProperty);
            set => SetValue(ContentUIProperty, value);
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e) => Deleted?.Invoke();
    }
}