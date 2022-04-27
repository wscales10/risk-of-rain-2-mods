using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WPFApp.Controls
{
	/// <summary>
	/// Interaction logic for ReplacementBorder.xaml
	/// </summary>
	public partial class ReplacementBorder : UserControl
	{
		public ReplacementBorder() => InitializeComponent();

		public UIElement Child
		{
			get => border.Child;

			set
			{
				BindingOperations.ClearBinding(border, Border.BorderThicknessProperty);
				BindingOperations.ClearBinding(border, Border.BorderBrushProperty);

				if (value is Control c)
				{
					Binding binding1 = new(nameof(c.BorderThickness))
					{
						Source = c,
						Mode = BindingMode.OneWay
					};

					border.SetBinding(Border.BorderThicknessProperty, binding1);

					Binding binding2 = new(nameof(c.BorderBrush))
					{
						Source = c,
						Mode = BindingMode.OneWay
					};

					border.SetBinding(Border.BorderBrushProperty, binding2);
				}

				border.Child = value;
			}
		}
	}
}