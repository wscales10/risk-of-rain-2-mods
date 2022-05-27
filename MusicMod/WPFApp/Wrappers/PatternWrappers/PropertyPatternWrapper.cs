using Patterns;
using Patterns.Patterns;
using WPFApp.Controls.PatternControls;
using Utils.Reflection;
using WPFApp.Wrappers.SaveResults;

namespace WPFApp.Wrappers.PatternWrappers
{
    internal class PropertyPatternWrapper<TObject> : PatternWrapper<PropertyPattern<TObject>, PropertyPatternControl>
    {
        protected PropertyPatternWrapper(PropertyPattern<TObject> pattern, NavigationContext navigationContext)
        {
            UIElement.NavigationContext = navigationContext;
            SetValue(pattern);
            UIElement.ValueChanged += NotifyValueChanged;
        }

        public override PropertyPatternControl UIElement { get; } = new(typeof(TObject));

        public override bool NeedsLeftMargin => false;

        protected override void setStatus(bool? status) => Outline(UIElement.propertyComboBox, status != false || UIElement.SelectedProperty is not null);

        protected override void setValue(PropertyPattern<TObject> value)
        {
            UIElement.WaitingPattern = value?.Pattern;
            UIElement.SelectedProperty = value?.PropertyInfo;
        }

        protected override SaveResult<PropertyPattern<TObject>> tryGetValue(GetValueRequest request)
        {
            var propertyInfo = UIElement.SelectedProperty;

            if (propertyInfo is null)
            {
                return new(false);
            }

            var result = UIElement.TryGetPattern(request);

            var value = result.IsSuccess
                ? typeof(PropertyPattern<TObject>)
                    .GetMethod("Create", new[] { typeof(string), typeof(IPattern) })
                    .MakeGenericMethod(propertyInfo.Type)
                    .InvokeStatic<PropertyPattern<TObject>>(propertyInfo.Name, result.Value)
                : null;

            return new(result, value);
        }
    }
}