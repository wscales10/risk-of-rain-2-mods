namespace WPFApp.ViewModels
{
    public abstract class NamedViewModelBase<T> : RowViewModelBase<T>
    {
        protected NamedViewModelBase(T item, NavigationContext navigationContext) : base(item, navigationContext)
        {
            NameResult.OnValidate += NameResult_OnValidate;
        }

        public override string NameWatermark => $"Unnamed {ItemTypeName}";

        public override string AsString => Name ?? base.AsString;

        protected virtual bool ValidateName(string name) => name?.Trim().Length != 0;

        private void NameResult_OnValidate(object sender, MyValidationEventArgs2<string> e)
        {
            if (!ValidateName(e.Value))
            {
                e.Invalidate();
            }
        }
    }
}