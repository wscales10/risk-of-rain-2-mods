using System.Windows;
using System.Windows.Input;
using WPFApp.ViewModels;

namespace WPFApp.Views
{
	/// <summary>
	/// Interaction logic for RuleTypePickerView.xaml
	/// </summary>
	public partial class RuleTypePickerView : Window
	{
		public RuleTypePickerView()
		{
			ViewModel = new(Complete);
			InitializeComponent();
		}

		public RuleTypePickerViewModel ViewModel
		{
			get => DataContext as RuleTypePickerViewModel;
			set => DataContext = value;
		}

		private void Complete(bool result)
		{
			DialogResult = result;
			Close();
		}

		private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e) => ViewModel.OkCommand.Execute(null);

		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter && ViewModel?.SelectedTypePair is not null && !CancelButton.IsFocused)
			{
				ViewModel.OkCommand.Execute(null);
			}
		}
	}
}