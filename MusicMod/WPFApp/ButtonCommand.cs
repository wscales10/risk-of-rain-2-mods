using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Utils.Reflection.Properties;

namespace WPFApp
{
	public class BindingPredicate
	{
		public BindingPredicate(INotifyPropertyChanged bindingSource, string propertyName, Func<object, bool> predicate = null)
		{
			BindingSource = bindingSource;
			PropertyName = propertyName;
			Predicate = predicate ?? (obj => (bool)obj);
		}

		public INotifyPropertyChanged BindingSource { get; }

		public string PropertyName { get; }

		public Func<object, bool> Predicate { get; }
	}

	public class ButtonCommand : ButtonCommandBase
	{
		private bool canExecute = true;

		public ButtonCommand(Action<object> action, bool canExecute = true) : base(action) => this.canExecute = canExecute;

		public ButtonCommand(Action<object> action, INotifyPropertyChanged bindingSource, string propertyName, Func<object, bool> predicate = null) : this(action, new BindingPredicate(bindingSource, propertyName, predicate))
		{
		}

		public ButtonCommand(Action<object> action, params BindingPredicate[] bindingPredicates) : base(action)
		{
			canExecute = bindingPredicates.All(bp => bp.Predicate(bp.BindingSource.GetPropertyValue(bp.PropertyName)));

			foreach (BindingPredicate bp in bindingPredicates)
			{
				bp.BindingSource.PropertyChanged += (s, e) =>
				{
					if (e.PropertyName == bp.PropertyName)
					{
						CanExecute = bp.Predicate(s.GetPropertyValue(bp.PropertyName));
					}
				};
			}
		}

		public bool CanExecute
		{
			get => canExecute;

			set
			{
				canExecute = value;
				updateCanExecute();
			}
		}

		public override void Execute(object parameter) => action(parameter);

		protected override bool GetCanExecute() => CanExecute;
	}

	public class ButtonContext : NotifyPropertyChangedBase
	{
		private ICommand command;

		private string label;

		public ICommand Command
		{
			get => command;
			set => SetProperty(ref command, value);
		}

		public string Label
		{
			get => label;
			set => SetProperty(ref label, value);
		}
	}

	public abstract class ButtonCommandBase : ICommand
	{
		protected ButtonCommandBase(Action<object> action) => this.action = action;

		public event EventHandler CanExecuteChanged;

		protected Action<object> action { get; }

		public abstract void Execute(object parameter);

		bool ICommand.CanExecute(object parameter) => GetCanExecute();

		protected abstract bool GetCanExecute();

		protected void updateCanExecute() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
	}

	public class ButtonCommand2 : ButtonCommandBase
	{
		private readonly Func<bool> getCanExecute;

		public ButtonCommand2(Action<object> action, Func<bool> getCanExecute) : base(action) => this.getCanExecute = getCanExecute;

		public override void Execute(object parameter) => action(parameter);

		public void UpdateCanExecute() => updateCanExecute();

		protected override bool GetCanExecute() => getCanExecute();
	}
}