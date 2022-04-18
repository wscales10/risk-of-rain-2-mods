using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WPFApp.Controls.PatternControls;

namespace WPFApp.Controls.GridManagers
{
	public class PatternContainerManager : GridManager<PatternContainer>
	{
		public PatternContainerManager(Grid grid) : base(grid)
		{
		}

		protected override double RowMinHeight => 0;

		protected override IEnumerable<UIElement> GetUIElements(PatternContainer item)
		{
			yield return item;
		}

		protected override LinkedListNode<PatternContainer> add(PatternContainer row, bool isDefault)
		{
			row.Deleted += Remove;
			return base.add(row, isDefault);
		}
	}
}