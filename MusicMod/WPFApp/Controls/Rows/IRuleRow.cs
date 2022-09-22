using MyRoR2;
using Rules.RuleTypes.Mutable;
using System.ComponentModel;
using WPFApp.ViewModels;

namespace WPFApp.Controls.Rows
{
    public interface IRuleRow : IRow
    {
        string Label { get; }

        NavigationViewModelBase OutputViewModel { get; }

        Rule<Context> Output { get; }

        ICollectionView Children { get; }

        IRuleRow Parent { get; set; }
    }
}