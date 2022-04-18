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

		public PatternContainer(IReadableControlWrapper patternWrapper, IndexGetter<PatternContainer> indexGetter) : this()
		{
			PatternWrapper = patternWrapper;
			Node = indexGetter(this);
		}

		public event Action<int> Deleted;

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

		public int? Node { get; }

		private void deleteButton_Click(object sender, RoutedEventArgs e) => Deleted?.Invoke(Node.Value);
	}
}