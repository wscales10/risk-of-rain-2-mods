using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using Utils;
using WPFApp.Controls.RuleControls;
using WPFApp.ViewModels;
using WPFApp.Controls;

namespace CustomControls.Converters
{
    [ValueConversion(typeof(ViewModelBase), typeof(UserControl))]
    public class ViewModelToControlConverter : SimpleValueConverter<ViewModelBase, UserControl>
    {
        private readonly Cache<Type, UserControl> cache = new(GetUserControl);

        public object ConvertBack(object value, object parameter,
            CultureInfo culture) => (value as UserControl)?.DataContext as ViewModelBase;

        protected override UserControl Convert(ViewModelBase value, object parameter,
                    CultureInfo culture)
        {
            if (!targetType.IsAssignableFrom(typeof(UserControl)))
            {
                throw new InvalidOperationException("The target must be a UserControl");
            }

            if (value is null)
            {
                return null;
            }

            UserControl output = cache[value.GetType()];
            output.DataContext = value;
            return output;
        }

        private static UserControl GetUserControl(Type viewModelType)
        {
            var output = new MyListView();

            switch (viewModelType.Name)
            {
                case nameof(SwitchRuleViewModel):
                    output.HelperContent = new SwitchRuleControl();
                    break;

                case nameof(BucketViewModel):
                    output.HelperContent = new BucketControl();
                    break;
            }

            return output;
        }
    }
}