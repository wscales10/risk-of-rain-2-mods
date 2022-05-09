using System.Windows;
using System.Windows.Controls;
using WPFApp.Controls.Rows;

namespace WPFApp.Controls.RuleControls
{
    public class RuleControlBase : UserControl
    {
        public static readonly DependencyProperty RowControlProperty = DependencyProperty.Register
        (
            nameof(RowControl),
            typeof(RowControl),
            typeof(RuleControlBase),
            new PropertyMetadata(null)
        );

        public RowControl RowControl
        {
            get => (RowControl)GetValue(RowControlProperty);
            set => SetValue(RowControlProperty, value);
        }
    }
}