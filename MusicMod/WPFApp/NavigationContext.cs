using Patterns;
using Rules.RuleTypes.Mutable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using WPFApp.ViewModels;

namespace WPFApp
{
	public interface INavigationContext : INotifyPropertyChanged
	{
		bool IsHome { get; }

		void GoHome();

		bool GoUp();

		bool GoUp(int count);

		NavigationViewModelBase GoInto(IEnumerable<Rule> sequence);

		NavigationViewModelBase GoInto(Rule rule);

		NavigationViewModelBase GoInto(IPattern pattern);

		NavigationViewModelBase GetViewModel(Rule rule);

		NavigationViewModelBase GetViewModel(IPattern pattern);

		void GoInto(NavigationViewModelBase viewModel);
	}

	public class MutableNavigationContext : NotifyPropertyChangedBase, INavigationContext
	{
		private bool isHome;

		public event Func<IEnumerable<object>, NavigationViewModelBase> OnGoInto;

		public event Action OnGoHome;

		public event Func<int, bool> OnGoUp;

		public event Func<object, NavigationViewModelBase> ViewModelRequested;

		public bool IsHome
		{
			get => isHome;

			set => SetProperty(ref isHome, value);
		}

		public void GoHome() => OnGoHome?.Invoke();

		public NavigationViewModelBase GoInto(Rule rule) => GoInto(new[] { rule });

		public NavigationViewModelBase GoInto(IEnumerable<Rule> sequence) => OnGoInto?.Invoke(sequence);

		public NavigationViewModelBase GoInto(IPattern pattern) => OnGoInto?.Invoke(new[] { pattern });

		public bool GoUp(int count) => OnGoUp?.Invoke(count) ?? false;

		public bool GoUp() => GoUp(1);

		public void GoInto(NavigationViewModelBase viewModel) => OnGoInto?.Invoke(new[] { viewModel });

		public NavigationViewModelBase GetViewModel(Rule rule) => ViewModelRequested?.Invoke(rule);

		public NavigationViewModelBase GetViewModel(IPattern pattern) => ViewModelRequested?.Invoke(pattern);
	}

	public class NavigationContext : NotifyPropertyChangedBase, INavigationContext
	{
		private readonly MutableNavigationContext mutable;

		public NavigationContext(MutableNavigationContext mutable) => SubscribeTo(this.mutable = mutable);

		public bool IsHome => mutable.IsHome;

		public NavigationViewModelBase GoInto(IEnumerable<Rule> sequence) => mutable.GoInto(sequence);

		public void GoInto(NavigationViewModelBase viewModel) => mutable.GoInto(viewModel);

		public NavigationViewModelBase GoInto(IPattern pattern) => mutable.GoInto(pattern);

		public void GoHome() => mutable.GoHome();

		public bool GoUp(int count) => mutable.GoUp(count);

		public bool GoUp() => mutable.GoUp();

		public NavigationViewModelBase GoInto(Rule rule) => mutable.GoInto(rule);

		public NavigationViewModelBase GetViewModel(Rule rule) => mutable.GetViewModel(rule);

		public NavigationViewModelBase GetViewModel(IPattern pattern) => mutable.GetViewModel(pattern);
	}
}