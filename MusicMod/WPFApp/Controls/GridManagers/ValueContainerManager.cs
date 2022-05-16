namespace WPFApp.Controls.GridManagers
{
    public class ValueContainerManager : GridManager<ValueContainer>
    {
        public ValueContainerManager() => BeforeItemAdded += (row, _, __) => ValueContainerManager_BeforeItemAdded(row);

        private void ValueContainerManager_BeforeItemAdded(ValueContainer row) => row.Deleted += () => Remove(row);
    }
}