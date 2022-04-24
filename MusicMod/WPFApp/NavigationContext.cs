using Patterns;
using Rules.RuleTypes.Mutable;
using System;
using System.ComponentModel;
using WPFApp.Controls;

namespace WPFApp
{
	public interface INavigationContext : INotifyPropertyChanged
	{
		bool IsHome { get; }

		void GoHome();

		void GoUp();

		ControlBase GoInto(Rule rule);

		ControlBase GoInto(IPattern pattern);
	}

	public class MutableNavigationContext : NotifyPropertyChangedBase, INavigationContext
	{
		private bool isHome;

		public event Func<object, ControlBase> OnGoInto;

		public event Action OnGoHome;

		public event Action OnGoUp;

		public bool IsHome
		{
			get => isHome;

			set
			{
				isHome = value;
				NotifyPropertyChanged();
			}
		}

		public void GoHome() => OnGoHome?.Invoke();

		public ControlBase GoInto(Rule rule) => OnGoInto?.Invoke(rule);

		public ControlBase GoInto(IPattern pattern) => OnGoInto?.Invoke(pattern);

		public void GoUp() => OnGoUp?.Invoke();
	}

	public class NavigationContext : NotifyPropertyChangedBase, INavigationContext
	{
		private readonly MutableNavigationContext mutable;

		public NavigationContext(MutableNavigationContext mutable) => SubscribeTo(this.mutable = mutable);

		public bool IsHome => mutable.IsHome;

		public ControlBase GoInto(Rule rule) => mutable.GoInto(rule);

		public ControlBase GoInto(IPattern pattern) => mutable.GoInto(pattern);

		public void GoHome() => mutable.GoHome();

		public void GoUp() => mutable.GoUp();
	}
}