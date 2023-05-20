using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Utils;
using WPFApp.ViewModels;

namespace WPFApp
{
    public abstract class Navigation : IChange<Navigation>
    {
        protected Navigation()
        {
        }

        public abstract Navigation Reversed { get; }
    }

    public abstract class SingleDirectionNavigation : Navigation
    {
        protected SingleDirectionNavigation(IEnumerable<NavigationViewModelBase> viewModels) => ViewModels = new ReadOnlyCollection<NavigationViewModelBase>(viewModels.ToList());

        public ReadOnlyCollection<NavigationViewModelBase> ViewModels { get; }
    }

    public class CompoundNavigation : Navigation
    {
        internal CompoundNavigation(IEnumerable<Navigation> navigations)
        {
            Navigations = navigations.ToReadOnlyCollection();
        }

        public ReadOnlyCollection<Navigation> Navigations { get; }

        public override Navigation Reversed => new CompoundNavigation(Navigations.Select(n => n.Reversed).Reverse());
    }

    public class AddNavigation : SingleDirectionNavigation
    {
        internal AddNavigation(IEnumerable<NavigationViewModelBase> viewModels) : base(viewModels)
        {
        }

        internal AddNavigation(params NavigationViewModelBase[] viewModels) : base(viewModels)
        {
        }

        public override Navigation Reversed => new RemoveNavigation(ViewModels.Reverse());
    }

    public class RemoveNavigation : SingleDirectionNavigation
    {
        internal RemoveNavigation(IEnumerable<NavigationViewModelBase> viewModels) : base(viewModels)
        {
        }

        internal RemoveNavigation(params NavigationViewModelBase[] viewModels) : base(viewModels)
        {
        }

        public override Navigation Reversed => new AddNavigation(ViewModels.Reverse());
    }
}