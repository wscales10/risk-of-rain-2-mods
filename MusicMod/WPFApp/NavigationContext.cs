using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using WPFApp.Controls.Rows;
using WPFApp.ViewModels;

namespace WPFApp
{
	public interface INavigationContext : INotifyPropertyChanged
	{
		bool IsHome { get; }

		string Path { get; }

		void GoHome();

		bool GoUp();

		bool GoUp(int count);

		NavigationViewModelBase GoInto(object obj);

		NavigationViewModelBase GetViewModel(object obj);

		bool NavigateTreeTo(IRuleRow ruleRow);

		XElement GetClipboardItem();

		string GetLabel(NavigationViewModelBase viewModel);

		void Register(NavigationViewModelBase viewModel, IRuleRow ruleRow);
	}

	public class MutableNavigationContext : NotifyPropertyChangedBase, INavigationContext
	{
		private readonly Dictionary<NavigationViewModelBase, IRuleRow> register = new();

		private bool isHome;

		private string path;

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

		public string Path
		{
			get => path;

			set => SetProperty(ref path, value);
		}

		public void GoHome() => OnGoHome?.Invoke();

		public NavigationViewModelBase GoInto(object obj) => OnGoInto?.Invoke(obj);

		public bool GoUp(int count) => OnGoUp?.Invoke(count) ?? false;

		public bool GoUp() => GoUp(1);

		public NavigationViewModelBase GetViewModel(object obj) => ViewModelRequested?.Invoke(obj);

		public bool NavigateTreeTo(IRuleRow ruleRow) => TreeNavigationRequested?.Invoke(ruleRow) ?? false;

		public XElement GetClipboardItem() => ClipboardItemRequested?.Invoke();

		public string GetLabel(NavigationViewModelBase viewModel)
		{
			if (register.TryGetValue(viewModel, out var ruleRow))
			{
				return ruleRow?.Label;
			}
			else
			{
				return null;
			}
		}

		public void Register(NavigationViewModelBase viewModel, IRuleRow ruleRow)
		{
			if (viewModel is null)
			{
				throw new ArgumentNullException(nameof(viewModel));
			}

			register[viewModel] = ruleRow;
		}
	}

	public class NavigationContext : NotifyPropertyChangedBase, INavigationContext
	{
		private readonly MutableNavigationContext mutable;

		public NavigationContext(MutableNavigationContext mutable) => SubscribeTo(this.mutable = mutable);

		public bool IsHome => mutable.IsHome;

		public string Path => mutable.Path;

		public NavigationViewModelBase GoInto(object obj) => mutable.GoInto(obj);

		public void GoHome() => mutable.GoHome();

		public bool GoUp(int count) => mutable.GoUp(count);

		public bool GoUp() => mutable.GoUp();

		public NavigationViewModelBase GetViewModel(object obj) => mutable.GetViewModel(obj);

		public bool NavigateTreeTo(IRuleRow ruleRow) => mutable.NavigateTreeTo(ruleRow);

		public XElement GetClipboardItem() => ((INavigationContext)mutable).GetClipboardItem();

		public string GetLabel(NavigationViewModelBase viewModel) => mutable.GetLabel(viewModel);

		public void Register(NavigationViewModelBase viewModel, IRuleRow ruleRow) => mutable.Register(viewModel, ruleRow);
	}
}