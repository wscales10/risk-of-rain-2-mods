using Patterns;
using Rules.RuleTypes.Mutable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using WPFApp.Controls;
using WPFApp.Controls.RuleControls;

namespace WPFApp
{
	public interface INavigationContext : INotifyPropertyChanged
	{
		bool IsHome { get; }

		void GoHome();

		void GoUp();

		void GoUp(int count);

		ControlBase GoInto(IEnumerable<Rule> sequence);

		ControlBase GoInto(Rule rule);

		ControlBase GoInto(IPattern pattern);

		RuleControlBase GetControl(Rule rule);

		ItemControlBase GetControl(IPattern pattern);
	}

	public class MutableNavigationContext : NotifyPropertyChangedBase, INavigationContext
	{
		private bool isHome;

		public event Func<IEnumerable<object>, ControlBase> OnGoInto;

		public event Action OnGoHome;

		public event Action<int> OnGoUp;

		public event Func<object, ControlBase> OnControlRequested;

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

		public ControlBase GoInto(Rule rule) => GoInto(new[] { rule });

		public ControlBase GoInto(IEnumerable<Rule> sequence) => OnGoInto?.Invoke(sequence);

		public ControlBase GoInto(IPattern pattern) => OnGoInto?.Invoke(new[] { pattern });

		public void GoUp(int count) => OnGoUp?.Invoke(count);

		public void GoUp() => GoUp(1);

		public RuleControlBase GetControl(Rule rule) => (RuleControlBase)(OnControlRequested?.Invoke(rule));

		public ItemControlBase GetControl(IPattern pattern) => (ItemControlBase)(OnControlRequested?.Invoke(pattern));
	}

	public class NavigationContext : NotifyPropertyChangedBase, INavigationContext
	{
		private readonly MutableNavigationContext mutable;

		public NavigationContext(MutableNavigationContext mutable) => SubscribeTo(this.mutable = mutable);

		public bool IsHome => mutable.IsHome;

		public ControlBase GoInto(IEnumerable<Rule> sequence) => mutable.GoInto(sequence);

		public ControlBase GoInto(IPattern pattern) => mutable.GoInto(pattern);

		public void GoHome() => mutable.GoHome();

		public void GoUp(int count) => mutable.GoUp(count);

		public void GoUp() => mutable.GoUp();

		public ControlBase GoInto(Rule rule) => mutable.GoInto(rule);

		public RuleControlBase GetControl(Rule rule) => mutable.GetControl(rule);

		public ItemControlBase GetControl(IPattern pattern) => mutable.GetControl(pattern);
	}
}