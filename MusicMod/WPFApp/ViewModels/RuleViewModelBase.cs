using Rules.RuleTypes.Mutable;
using System.Xml.Linq;
using WPFApp.SaveResults;

namespace WPFApp.ViewModels
{
    public abstract class RuleViewModelBase<TRule> : NamedViewModelBase<TRule>, IXmlViewModel
        where TRule : Rule
    {
        protected RuleViewModelBase(TRule rule, NavigationContext navigationContext) : base(rule, navigationContext)
        {
            Name = Item.Name;
        }

        public XElement GetContentXml() => Item.ToXml();

        protected override SaveResult<TRule> ShouldAllowExit()
        {
            NameResult.MaybeOutput(name => Item.Name = name);
            NotifyPropertyChanged(nameof(Name));
            return base.ShouldAllowExit() & NameResult;
        }
    }
}