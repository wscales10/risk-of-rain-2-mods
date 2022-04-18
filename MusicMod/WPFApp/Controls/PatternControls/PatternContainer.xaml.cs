using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using WPFApp.Controls.Rows;
using WPFApp.Controls.Wrappers;

namespace WPFApp.Controls.PatternControls
{
	/// <summary>
	/// Interaction logic for PatternContainer.xaml
	/// </summary>
	public partial class PatternContainer : UserControl
	{
		private IReadableControlWrapper patternWrapper;

		public PatternContainer()
		{
			InitializeComponent();

			Binding binding = new(nameof(IsMouseOver))
			{
				Source = this,
				Converter = new Converters.BooleanToVisibilityConverter(false)
			};

			_ = deleteButton.SetBinding(VisibilityProperty, binding);
		}

		public PatternContainer(IReadableControlWrapper patternWrapper) : this() => PatternWrapper = patternWrapper;

		public event Action Deleted;

		public IReadableControlWrapper PatternWrapper
		{
			get => patternWrapper;

			set
			{
				FrameworkElement ui = (patternWrapper = value)?.UIElement;

				if (ui is not null)
				{
					ui.HorizontalAlignment = HorizontalAlignment.Center;
					ui.VerticalAlignment = VerticalAlignment.Center;
				}

				border.Child = ui;
			}
		}

		private void deleteButton_Click(object sender, RoutedEventArgs e) => Deleted?.Invoke();
	}
}