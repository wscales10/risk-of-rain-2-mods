using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace WPFApp.Controls
{
    public class MyComboBox : ComboBox
    {
        public static readonly DependencyProperty LinkTextProperty = DependencyProperty.Register
        (
            nameof(LinkText),
            typeof(bool),
            typeof(MyComboBox),
            new PropertyMetadata(true)
        );

        private bool ignore = false;

        public bool LinkText
        {
            get => (bool)GetValue(LinkTextProperty);
            set => SetValue(LinkTextProperty, value);
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (!ignore)
            {
                base.OnSelectionChanged(e);
            }
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!LinkText)
            {
                ignore = true;
            }

            try
            {
                base.OnItemsChanged(e);
            }
            finally
            {
                ignore = false;
            }
        }
    }
}