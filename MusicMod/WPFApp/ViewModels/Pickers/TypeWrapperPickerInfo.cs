namespace WPFApp.ViewModels.Pickers
{
    internal abstract class TypeWrapperPickerInfo : PickerInfoBase<TypeWrapper>
    {
        protected TypeWrapperPickerInfo(NavigationContext navigationContext) : base(navigationContext)
        {
        }

        public override string DisplayMemberPath => nameof(TypeWrapper.DisplayName);

        public override string SelectedValuePath => nameof(TypeWrapper.Type);
    }
}