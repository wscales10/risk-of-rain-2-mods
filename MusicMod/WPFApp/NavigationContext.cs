using System;
using System.ComponentModel;
using System.Xml.Linq;
using WPFApp.Controls.Rows;
using WPFApp.ViewModels;

namespace WPFApp
{
	public interface INavigationContext : INotifyPropertyChanged
	{
		bool IsHome { get; }

		void GoHome();

		bool GoUp();

		bool GoUp(int count);

		NavigationViewModelBase GoInto(object obj);

		NavigationViewModelBase GetViewModel(object obj);

		bool NavigateTreeTo(IRuleRow ruleRow);

		XElement GetClipboardItem();
	}

	public class MutableNavigationContext : NotifyPropertyChangedBase, INavigationContext
	{
		private bool isHome;

		public event Func<object, NavigationViewModelBase> OnGoInto;

		public event Action OnGoHome;

		public event Func<int, bool> OnGoUp;

		public event Func<IRuleRow, bool> TreeNavigationRequested;

		public event Func<object, NavigationViewModelBase> ViewModelRequested;

		public event Func<XElement> ClipboardItemRequested;

		public bool IsHome
		{
			get => isHome;

			set => SetProperty(ref isHome, value);
		}

		public void GoHome() => OnGoHome?.Invoke();

		public NavigationViewModelBase GoInto(object obj) => OnGoInto?.Invoke(obj);

		public bool GoUp(int count) => OnGoUp?.Invoke(count) ?? false;

		public bool GoUp() => GoUp(1);

		public NavigationViewModelBase GetViewModel(object obj) => ViewModelRequested?.Invoke(obj);

		public bool NavigateTreeTo(IRuleRow ruleRow) => TreeNavigationRequested?.Invoke(ruleRow) ?? false;

		public XElement GetClipboardItem() => ClipboardItemRequested?.Invoke();
	}

	public class NavigationContext : NotifyPropertyChangedBase, INavigationContext
	{
		private readonly MutableNavigationContext mutable;

		public NavigationContext(MutableNavigationContext mutable) => SubscribeTo(this.mutable = mutable);

		public bool IsHome => mutable.IsHome;

		public NavigationViewModelBase GoInto(object obj) => mutable.GoInto(obj);

		public void GoHome() => mutable.GoHome();

		public bool GoUp(int count) => mutable.GoUp(count);

		public bool GoUp() => mutable.GoUp();

		public NavigationViewModelBase GetViewModel(object obj) => mutable.GetViewModel(obj);

		public bool NavigateTreeTo(IRuleRow ruleRow) => mutable.NavigateTreeTo(ruleRow);

		public XElement GetClipboardItem() => ((INavigationContext)mutable).GetClipboardItem();
	}
}