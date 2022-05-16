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

        public static readonly DependencyProperty LeftColumnWidthProperty = DependencyProperty.Register
        (
            nameof(LeftColumnWidth),
            typeof(GridLength),
            typeof(Aligner),
            new PropertyMetadata(new GridLength())
        );

        public static readonly DependencyProperty RightColumnWidthProperty = DependencyProperty.Register
        (
            nameof(RightColumnWidth),
            typeof(GridLength),
            typeof(Aligner),
            new PropertyMetadata(new GridLength())
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

        public Aligner()
        {
            InitializeComponent();
        }

        public HorizontalAlignment Alignment
        {
            get => (HorizontalAlignment)GetValue(AlignmentProperty);

            set
            {
                SetValue(AlignmentProperty, value);
                switch (value)
                {
                    case HorizontalAlignment.Left:
                        LeftColumnWidth = new GridLength(0);
                        RightColumnWidth = GridLength.Auto;
                        break;

                    case HorizontalAlignment.Center:
                        LeftColumnWidth = GridLength.Auto;
                        RightColumnWidth = GridLength.Auto;
                        break;

                    case HorizontalAlignment.Right:
                        LeftColumnWidth = GridLength.Auto;
                        RightColumnWidth = new GridLength(0);
                        break;

                    case HorizontalAlignment.Stretch:
                        LeftColumnWidth = new GridLength(0);
                        RightColumnWidth = new GridLength(0);
                        break;
                }
            }
        }

        public GridLength LeftColumnWidth
        {
            get => (GridLength)GetValue(LeftColumnWidthProperty);
            private set => SetValue(LeftColumnWidthProperty, value);
        }

        public GridLength RightColumnWidth
        {
            get => (GridLength)GetValue(RightColumnWidthProperty);
            private set => SetValue(RightColumnWidthProperty, value);
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