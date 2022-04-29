using System;
using System.ComponentModel;
using System.Windows.Input;
using Utils.Reflection.Properties;

namespace WPFApp
{
	public class ButtonCommand : ICommand
	{
		private readonly Action<object> action;

		private bool canExecute = true;

		public ButtonCommand(Action<object> action, bool canExecute = true)
		{
			this.action = action;
			this.canExecute = canExecute;
		}

		public ButtonCommand(Action<object> action, INotifyPropertyChanged bindingSource, string propertyName, Func<object, bool> predicate = null)
		{
			predicate ??= obj => (bool)obj;
			this.action = action;
			canExecute = predicate(bindingSource.GetPropertyValue(propertyName));
			bindingSource.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == propertyName)
				{
					CanExecute = predicate(s.GetPropertyValue(propertyName));
				}
			};
		}

		public event EventHandler CanExecuteChanged;

		public bool CanExecute
		{
			get => canExecute;

			set
			{
				canExecute = value;
				CanExecuteChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public void Execute(object parameter) => action(parameter);

		bool ICommand.CanExecute(object parameter) => CanExecute;
	}
}