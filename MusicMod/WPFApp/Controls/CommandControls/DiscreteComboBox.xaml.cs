using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace WPFApp.Controls.CommandControls
{
	/// <summary>
	/// Interaction logic for DiscreteComboBox.xaml
	/// </summary>
	public partial class DiscreteComboBox : UserControl
	{
		public DiscreteComboBox()
		{
			InitializeComponent();
			button.Click += Button_Click;
			LostFocus += DiscreteComboBox_LostFocus;
			ListBox.SelectionChanged += ListBox_SelectionChanged;
			popup.PlacementTarget = button;
			popup.Placement = PlacementMode.Bottom;
		}

		public DiscreteComboBox(HorizontalAlignment alignment, string displayMemberPath) : this()
		{
			Alignment = alignment;
			DisplayMemberPath = displayMemberPath;
		}

		internal event Action<object> OnSelectionMade;

		public HorizontalAlignment Alignment
		{
			set
			{
				button.HorizontalAlignment = value;
				button.VerticalAlignment = value == HorizontalAlignment.Center ? VerticalAlignment.Top : VerticalAlignment.Center;
				BindingOperations.ClearBinding(popup, Popup.HorizontalOffsetProperty);

				switch (value)
				{
					case HorizontalAlignment.Center:
						MultiBinding binding = new() { Converter = new Converters.CenterPopupConverter() };
						binding.Bindings.Add(new Binding(nameof(ActualWidth)) { Source = button });
						binding.Bindings.Add(new Binding(nameof(ActualWidth)) { Source = ListBox });
						popup.SetBinding(Popup.HorizontalOffsetProperty, binding);
						break;
					case HorizontalAlignment.Left:
						popup.HorizontalOffset = 0;
						break;
					default:
						throw new NotSupportedException();
				}
			}
		}

		public IEnumerable ItemsSource
		{
			set => ListBox.ItemsSource = value;
		}

		public string DisplayMemberPath
		{
			set => ListBox.DisplayMemberPath = value;
		}

		private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => OnSelectionMade?.Invoke(((ListBox)sender).SelectedItem);

		private void DiscreteComboBox_LostFocus(object sender, RoutedEventArgs e) => popup.IsOpen = false;

		private void Button_Click(object sender, RoutedEventArgs e) => popup.IsOpen = true;
	}
}