using Patterns;
using Rules.RuleTypes.Mutable;
using System;
using System.ComponentModel;
using WPFApp.Controls;
using WPFApp.Properties;

namespace WPFApp
{
	public class MutableNavigationContext : INotifyPropertyChanged
	{
		public event Func<object, ControlBase> OnGoInto;

		public event PropertyChangedEventHandler PropertyChanged;

		public bool IsOffline
		{
			get => Settings.Default.OfflineMode;

			set
			{
				Settings.Default.OfflineMode = value;
				Settings.Default.Save();
				OnPropertyChanged(nameof(IsOffline));
			}
		}

		public ControlBase GoInto(Rule rule) => OnGoInto?.Invoke(rule);

		public ControlBase GoInto(IPattern pattern) => OnGoInto?.Invoke(pattern);

		private void OnPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
	}

	public class NavigationContext : INotifyPropertyChanged
	{
		private readonly MutableNavigationContext mutable;

		public NavigationContext(MutableNavigationContext mutable)
		{
			this.mutable = mutable;
			mutable.PropertyChanged += (_, e) => PropertyChanged?.Invoke(this, e);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public bool IsOffline
		{
			get => mutable.IsOffline;
			set => mutable.IsOffline = value;
		}

		public ControlBase GoInto(Rule rule) => mutable.GoInto(rule);

		public ControlBase GoInto(IPattern pattern) => mutable.GoInto(pattern);
	}
}