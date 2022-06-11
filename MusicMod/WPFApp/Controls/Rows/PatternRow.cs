using Patterns;
using System;
using System.Windows;
using Utils.Reflection;
using WPFApp.Controls.Wrappers;
using WPFApp.Controls.Wrappers.PatternWrappers;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Rows
{
    internal class PatternRow : Row<IPattern, PatternRow>
    {
        private readonly IControlWrapper singlePickerWrapper;

        private readonly Type valueType;

        private readonly NavigationContext navigationContext;

        public PatternRow(Type valueType, NavigationContext navigationContext) : base(true)
        {
            singlePickerWrapper = (IControlWrapper)typeof(SinglePatternPickerWrapper<>).MakeGenericType(valueType).Construct(navigationContext);
            this.valueType = valueType;
            this.navigationContext = navigationContext;
        }

        public override IPattern Output
        {
            get
            {
                singlePickerWrapper.ForceGetValue(out object obj);
                return (IPattern)obj;
            }

            set
            {
                singlePickerWrapper.SetValue(value);
                NotifyPropertyChanged();
            }
        }

        protected override SaveResult trySaveChanges() => singlePickerWrapper.TryGetObject(true);

        protected override UIElement MakeOutputUi()
        {
            var output = singlePickerWrapper?.UIElement;

            if (output is not null)
            {
                output.Margin = new Thickness(40, 4, 4, 4);

                // output.MinWidth = 150;
                output.HorizontalAlignment = HorizontalAlignment.Left;
            }

            return output;
        }

        protected override IPattern CloneOutput() => Info.PatternParser.DeepClone(Output);

        protected override PatternRow deepClone() => new(valueType, navigationContext);
    }
}