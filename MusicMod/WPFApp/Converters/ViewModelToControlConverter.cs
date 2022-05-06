using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using Utils;
using WPFApp.Controls.PatternControls;
using WPFApp.Controls.RuleControls;
using WPFApp.ViewModels;

namespace WPFApp.Converters
{
	[ValueConversion(typeof(ViewModelBase), typeof(UserControl))]
	public class ViewModelToControlConverter : IValueConverter
	{
		private static readonly Cache<Type, UserControl> cache = new(GetUserControl);

		public object Convert(object value, Type targetType, object parameter,
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

		public object ConvertBack(object value, Type targetType, object parameter,
			CultureInfo culture) => (value as UserControl)?.DataContext as ViewModelBase;

		private static UserControl GetUserControl(Type viewModelType) => viewModelType.Name switch
		{
			nameof(ArrayRuleViewModel) => new ArrayRuleControl(),
			nameof(BucketViewModel) => new BucketControl(),
			nameof(IfRuleViewModel) => new IfRuleControl(),
			nameof(ListPatternViewModel) => new ListPatternControl(),
			nameof(SwitchRuleViewModel) => new SwitchRuleControl(),
			_ => throw new NotImplementedException(),
		};
	}
}