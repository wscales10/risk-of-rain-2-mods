using System.Windows;
using System.Windows.Controls;
using WPFApp.Rows;

namespace WPFApp.Controls.RuleControls
{
    public class RuleControlBase : UserControl
    {
        public static readonly DependencyProperty MyListViewProperty = DependencyProperty.Register
        (
            nameof(MyListView),
            typeof(MyListView),
            typeof(RuleControlBase),
            new PropertyMetadata(null)
        );

        public MyListView MyListView
        {
            get => (MyListView)GetValue(MyListViewProperty);
            set => SetValue(MyListViewProperty, value);
        }
    }
}