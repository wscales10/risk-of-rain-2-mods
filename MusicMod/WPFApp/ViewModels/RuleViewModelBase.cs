using Rules.RuleTypes.Mutable;
using System.Xml.Linq;
using WPFApp.Controls.Rows;
using WPFApp.Controls.Wrappers;

namespace WPFApp.ViewModels
{
    public abstract class RuleViewModelBase<TRule> : NamedViewModelBase<TRule>, IXmlViewModel
        where TRule : Rule
    {
        private string name;

        protected RuleViewModelBase(TRule rule, NavigationContext navigationContext) : base(rule, navigationContext)
        {
            Name = Item.Name;
        }

        public override string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        public XElement GetContentXml() => Item.ToXml();

        internal void AttachRowEventHandlers<T>(RuleRow<T> row, bool _, int __)
                                        where T : RuleRow<T>
        {
            row.OutputViewModelRequestHandler = NavigationContext.GetViewModel;
            row.OnOutputButtonClick += NavigationContext.GoInto;
        }

        protected override SaveResult ShouldAllowExit()
        {
            Item.Name = Name?.Length > 0 ? Name : null;
            return base.ShouldAllowExit();
        }
    }
}