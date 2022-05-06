using WPFApp.Controls.PatternControls;

namespace WPFApp.Controls.GridManagers
{
	public class PatternContainerManager : GridManager<PatternContainer>
	{
		public PatternContainerManager() => BeforeItemAdded += (row, _, __) => PatternContainerManager_BeforeItemAdded(row);

		private void PatternContainerManager_BeforeItemAdded(PatternContainer row) => row.Deleted += () => Remove(row);
	}
}