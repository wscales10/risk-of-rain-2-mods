using System.Windows.Data;
using Utils.Reflection.Properties;

namespace WPFApp.Converters
{
    [ValueConversion(typeof(object), typeof(object))]
    public class DisplayMemberPathConverter : SimpleValueConverter<object, object>
    {
        public string DisplayMemberPath { get; set; }

        protected override object Convert(object value)
        {
            return value.GetDeepPropertyValue(DisplayMemberPath);
        }
    }
}