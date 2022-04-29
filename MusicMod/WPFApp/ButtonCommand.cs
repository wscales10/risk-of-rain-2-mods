using System;
using System.Windows.Input;

namespace WPFApp
{
	public class ButtonCommand : ICommand
	{
		private readonly Action<object> action;

		private bool isEnabled = true;

		public ButtonCommand(Action<object> action) => this.action = action;

		public event EventHandler CanExecuteChanged;

		public bool IsEnabled
		{
			get => isEnabled;

			set
			{
				isEnabled = value;
				CanExecuteChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public bool CanExecute(object parameter) => IsEnabled;

		public void Execute(object parameter) => action(parameter);
	}
}