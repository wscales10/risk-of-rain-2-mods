using Patterns;
using System.Xml.Linq;

namespace WPFApp.ViewModels
{
	public abstract class PatternViewModelBase<TPattern> : RowViewModelBase<TPattern>, IXmlViewModel where TPattern : IPattern
	{
		protected PatternViewModelBase(TPattern pattern, NavigationContext navigationContext) : base(pattern, navigationContext)
		{
		}

		public XElement GetContentXml() => Item.ToXml();
	}
}