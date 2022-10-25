using Patterns;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Utils;

using WPFApp.Controls.Wrappers;
using System.Windows;
using System.Windows.Media;

namespace WPFApp.Controls
{
	public class Pair<T> : NotifyPropertyChangedBase
	{
		private T item1;

		private T item2;

		public Pair(T item1, T item2)
		{
			Item1 = item1;
			Item2 = item2;
		}

		public T Item1 { get => item1; set => SetProperty(ref item1, value); }

		public T Item2 { get => item2; set => SetProperty(ref item2, value); }
	}

	/// <summary>
	/// Interaction logic for SwitchControl.xaml
	/// </summary>
	public partial class SwitchControl : UserControl
	{
		private readonly ObservableCollection<Pair<string>> rows = new();

		public SwitchControl()
		{
			InitializeComponent();
			ListBox.ItemsSource = rows;
		}

		public void SetValue<T, TOut>(Switch<T, TOut> value, Func<IPattern<T>, string> stringifier1, Func<TOut, string> stringifier2)
		{
			rows.Clear();

			foreach (var c in value.Cases)
			{
				rows.Add(new(stringifier2(c.Output), stringifier1(c.Arr.Single())));
			}

			if (value.HasDefault)
			{
				rows.Add(new(stringifier2(value.DefaultOut), "else"));
			}
		}

		public bool TryGetValue<T, TOut>(Parser<TOut> parser2, out Switch<T, TOut> output)
		{
			List<Case<IPattern<T>, TOut>> cases = new();
			TOut defaultOut = default;
			bool hasDefault = false;

			foreach (var row in rows)
			{
				if (!parser2(row.Item1, out var result))
				{
					output = null;
					return false;
				}

				string condition = row.Item2;

				if (condition == "else")
				{
					if (hasDefault)
					{
						this.Log("already has default");
					}

					defaultOut = result;
					hasDefault = true;
				}
				else
				{
					if (!Info.PatternParser.TryParse<T>(condition, out var pattern))
					{
						output = null;
						return false;
					}

					cases.Add(new(result, pattern));
				}
			}

			output = hasDefault
				? new Switch<T, TOut>(defaultOut, cases.ToArray())
				: new Switch<T, TOut>(cases.ToArray());
			return true;
		}

		private void Grid_Loaded(object sender, RoutedEventArgs e) => FocusTextBox(0);

		private void FocusTextBox(int index)
		{
			int[] indices = new[] { 0, 2 };
			var selectedItem = (ListBoxItem)ListBox.ItemContainerGenerator.ContainerFromIndex(Math.Max(ListBox.SelectedIndex, 0));

			if (!selectedItem.IsAncestorOf(Keyboard.FocusedElement as DependencyObject))
			{
				Keyboard.Focus(VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(selectedItem, 0), 0), 0), indices[index]) as IInputElement);
			}
		}

		private void TextBox1_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			var textBox = (TextBox)sender;
			var index = rows.IndexOf((Pair<string>)textBox.DataContext);

			if (e.Key == Key.Enter)
			{
				AddRow(sender);
			}
			else if (e.Key == Key.Back && textBox.CaretIndex == 0 && textBox.SelectionLength == 0 && ListBox.Items.Count > 1)
			{
				ListBox.SelectedIndex = index == rows.Count - 1 ? index - 1 : index + 1;
				FocusTextBox(1);
				rows.RemoveAt(index);
				e.Handled = true;
			}
		}

		private void AddRow(object sender, Pair<string> row = null)
		{
			var index = rows.IndexOf((Pair<string>)((TextBox)sender).DataContext);
			AddRow(index, row);
		}

		private void AddRow(int index, Pair<string> row = null)
		{
			row ??= new("", "");

			if (++index == rows.Count)
			{
				rows.Add(row);
			}
			else
			{
				rows.Insert(index, row);
			}

			ListBox.SelectedIndex++;
		}

		private void TextBox2_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				AddRow(sender);
			}
		}

		private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			ListBox.SelectedItem = ((TextBox)sender).DataContext;
		}

		private void ListBox_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (rows.Count == 0)
			{
				AddRow(-1, new("ms", "else"));
			}
		}
	}
}