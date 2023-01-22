using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Utils;

namespace WPFApp
{
	internal class PropertyInformation<T>
	{
		private readonly Func<T> getValue;

		public PropertyInformation(Func<T> getValue, params DependencyInformation[] dependencies)
		{
			this.getValue = getValue;
			Dependencies = dependencies.ToReadOnlyCollection();
		}

		public ReadOnlyCollection<DependencyInformation> Dependencies { get; }

		public T GetValue() => getValue();
	}

	internal class DependencyInformation
	{
		public DependencyInformation(params string[] dependentOn)
		{
			DependentOn = dependentOn.ToReadOnlyCollection();
		}

		public DependencyInformation(INotifyPropertyChanged source, params string[] dependentOn) : this(dependentOn)
		{
			Source = source;
		}

		public INotifyPropertyChanged Source { get; }

		public ReadOnlyCollection<string> DependentOn { get; }
	}
}