using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Utils;

namespace WPFApp
{
	public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
	{
		private readonly AutoInitialiseDictionary<string, HashSet<string>> dependencies = new();

		public event PropertyChangedEventHandler PropertyChanged;

		public void SubscribeTo(INotifyPropertyChanged obj) => obj.PropertyChanged += (s, e) => NotifyPropertyChanged(e);

		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
		{
			NotifyPropertyChanged(new PropertyChangedEventArgs(propertyName));

			foreach (string dependant in dependencies[propertyName])
			{
				NotifyPropertyChanged(dependant);
			}
		}

		protected void NotifyPropertyChanged(PropertyChangedEventArgs e) => PropertyChanged?.Invoke(this, e);

		protected void SetPropertyDependency(string propertyName, params string[] dependentOn)
		{
			foreach (HashSet<string> set in dependencies[dependentOn])
			{
				_ = set.Add(propertyName);
			}
		}
	}
}