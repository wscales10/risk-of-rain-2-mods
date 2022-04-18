using Spotify.Commands;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Utils;
using WPFApp.Converters;

namespace WPFApp.Controls.CommandControls
{
	internal sealed class FormatString
	{
		private readonly PropertyString[] propertyStrings;

		private readonly ObservableCollection<PropertyString> notDisplayed = new();

		private StackPanel propertiesPanel;

		internal FormatString(Type type, params PropertyString[] propertyStrings)
		{
			Type = type;
			this.propertyStrings = propertyStrings;
		}

		public string Name { get; }

		public Type Type { get; }

		internal static FormatString Create<T>(params PropertyString<T>[] propertyStrings) => new(typeof(T), propertyStrings);

		internal StackPanel BuildControl()
		{
			StackPanel output = new() { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(40, 4, 4, 4) };

			propertiesPanel = new() { Orientation = Orientation.Horizontal };

			foreach (var propertyString in propertyStrings)
			{
				_ = propertiesPanel.Children.Add(propertyString.UI);
				Hide(propertyString);

				if (propertyString.IsRequired)
				{
					Display(propertyString);
				}
				else
				{
					propertyString.FocusElement.KeyDown += (s, e) =>
					{
						switch (e.Key)
						{
							case Key.Delete:
								Hide(propertyString);
								ShiftFocus(-1);
								break;

							case Key.Left:
								ShiftFocus(-1);
								return;

							case Key.Right:
								ShiftFocus(1);
								return;
						}

						e.Handled = true;

						void ShiftFocus(int diff)
						{
							int index = Array.IndexOf(propertyStrings, propertyString);
							PropertyString neighbour;

							do
							{
								if (index == (diff > 0 ? propertyStrings.Length - 1 : 0))
								{
									return;
								}

								index += diff;
								neighbour = propertyStrings[index];
							} while (notDisplayed.Contains(neighbour));

							e.Handled = neighbour.FocusElement.Focus();
						}
					};
				}
			}

			_ = output.Children.Add(propertiesPanel);

			DiscreteComboBox add = new(HorizontalAlignment.Left, nameof(PropertyString.PropertyName));
			add.ItemsSource = notDisplayed;
			add.OnSelectionMade += ps => Display((PropertyString)ps);

			Binding myBinding = new(nameof(notDisplayed.Count))
			{
				Source = notDisplayed,
				Converter = new IntToVisibilityConverter(i => i > 0)
			};

			_ = add.SetBinding(UIElement.VisibilityProperty, myBinding);

			_ = output.Children.Add(add);
			return output;
		}

		internal void SetProperties(Command command)
		{
			foreach (var propertyString in propertyStrings)
			{
				object value = command.GetPropertyValue(propertyString.PropertyName);

				if (propertyString.IsRequired || value is not null)
				{
					propertyString.ControlWrapper.SetValue(value);
					Display(propertyString);
				}
				else
				{
					Hide(propertyString);
				}
			}
		}

		internal bool TryGetProperties(Command command)
		{
			bool success = true;

			foreach (var propertyString in propertyStrings)
			{
				if (!propertyString.IsRequired && notDisplayed.Contains(propertyString))
				{
					command.SetPropertyValue(propertyString.PropertyName, null);
				}
				else if (propertyString.ControlWrapper.TryGetValue(out object value))
				{
					command.SetPropertyValue(propertyString.PropertyName, value);
				}
				else
				{
					success = false;
				}
			}

			return success;
		}

		private void Display(PropertyString propertyString)
		{
			if (notDisplayed.Contains(propertyString))
			{
				_ = notDisplayed.Remove(propertyString);
				propertyString.UI.Visibility = Visibility.Visible;
			}
		}

		private void Hide(PropertyString propertyString)
		{
			if (!notDisplayed.Contains(propertyString))
			{
				notDisplayed.Add(propertyString);
				propertyString.UI.Visibility = Visibility.Collapsed;
			}
		}
	}
}