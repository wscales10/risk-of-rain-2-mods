using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WPFApp.ViewModels;

namespace WPFApp
{
	public abstract class Navigation : IChange<Navigation>
	{
		protected Navigation(IEnumerable<NavigationViewModelBase> controls) => ViewModels = new ReadOnlyCollection<NavigationViewModelBase>(controls.ToList());

		public ReadOnlyCollection<NavigationViewModelBase> ViewModels { get; }

		public abstract Navigation Reversed { get; }
	}

	public class AddNavigation : Navigation
	{
		internal AddNavigation(IEnumerable<NavigationViewModelBase> controls) : base(controls)
		{
		}

		internal AddNavigation(params NavigationViewModelBase[] controls) : base(controls)
		{
		}

		public override Navigation Reversed => new RemoveNavigation(ViewModels);
	}

	public class RemoveNavigation : Navigation
	{
		internal RemoveNavigation(IEnumerable<NavigationViewModelBase> controls) : base(controls)
		{
		}

		internal RemoveNavigation(params NavigationViewModelBase[] controls) : base(controls)
		{
		}

		public override Navigation Reversed => new AddNavigation(ViewModels);
	}
}