using System.Windows;
using System.Windows.Controls;

namespace WPFApp.Controls
{
    /// <summary>
    /// Interaction logic for Aligner.xaml
    /// </summary>
    public partial class Aligner : UserControl
    {
        public static readonly DependencyProperty AlignmentProperty = DependencyProperty.Register
        (
            nameof(Alignment),
            typeof(HorizontalAlignment),
            typeof(Aligner),
            new PropertyMetadata(HorizontalAlignment.Center)
        );

        public static readonly DependencyProperty ChildProperty = DependencyProperty.Register
        (
            nameof(Child),
            typeof(object),
            typeof(Aligner),
            new PropertyMetadata(null)
        );

        public static readonly DependencyProperty MaxChildWidthProperty = DependencyProperty.Register
        (
            nameof(MaxChildWidth),
            typeof(double),
            typeof(Aligner),
            new PropertyMetadata(0d)
        );

        internal static readonly DependencyPropertyKey LeftColumnWidthPropertyKey = DependencyProperty.RegisterReadOnly
        (
            nameof(LeftColumnWidth),
            typeof(GridLength),
            typeof(Aligner),
            new PropertyMetadata(new GridLength())
        );

        internal static readonly DependencyPropertyKey RightColumnWidthPropertyKey = DependencyProperty.RegisterReadOnly
        (
            nameof(RightColumnWidth),
            typeof(GridLength),
            typeof(Aligner),
            new PropertyMetadata(new GridLength())
        );

        public Aligner()
        {
            InitializeComponent();
        }

        public HorizontalAlignment Alignment
        {
            get => (HorizontalAlignment)GetValue(AlignmentProperty);

            set => SetValue(AlignmentProperty, value);
        }

        public GridLength LeftColumnWidth
        {
            get => (GridLength)GetValue(LeftColumnWidthPropertyKey.DependencyProperty);
            private set => SetValue(LeftColumnWidthPropertyKey, value);
        }

        public GridLength RightColumnWidth
        {
            get => (GridLength)GetValue(RightColumnWidthPropertyKey.DependencyProperty);
            private set => SetValue(LeftColumnWidthPropertyKey, value);
        }

        public object Child
        {
            get => GetValue(ChildProperty);
            set => SetValue(ChildProperty, value);
        }

        public double MaxChildWidth
        {
            get => (double)GetValue(MaxChildWidthProperty);
            set => SetValue(MaxChildWidthProperty, value);
        }
    }
}