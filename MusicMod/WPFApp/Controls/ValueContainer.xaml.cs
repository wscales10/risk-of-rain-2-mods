using System;
using System.Windows;
using System.Windows.Controls;

namespace WPFApp.Controls
{
    /// <summary>
    /// Interaction logic for ValueContainer.xaml
    /// </summary>
    public partial class ValueContainer : UserControl
    {
        public ValueContainer()
        {
            InitializeComponent();
        }

        public ValueContainer(IReadableControlWrapper valueWrapper) : this() => ValueWrapper = valueWrapper;

        public event Action Deleted;

        public IReadableControlWrapper ValueWrapper
        {
            get => (IReadableControlWrapper)DataContext;

            set
            {
                DataContext = value;
                FrameworkElement ui = value?.UIElement;

                if (ui is not null)
                {
                    ui.VerticalAlignment = VerticalAlignment.Center;
                }
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e) => Deleted?.Invoke();
    }
}