namespace WPFApp.ViewModels
{
    public abstract class NamedViewModelBase<T> : RowViewModelBase<T>, INamedViewModel
    {
        protected NamedViewModelBase(T item, NavigationContext navigationContext) : base(item, navigationContext)
        {
        }

        public abstract string Name { get; set; }

        public string NameWatermark => $"Unnamed {ItemTypeName}";
    }
}