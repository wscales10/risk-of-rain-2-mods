using System.Windows;
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

		private void Label_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			ViewModel.OkCommand.Execute(null);
		}
	}
}