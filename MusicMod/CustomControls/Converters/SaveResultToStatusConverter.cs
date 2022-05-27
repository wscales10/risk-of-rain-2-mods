using System.Windows.Data;
using WPFApp.SaveResults;

namespace CustomControls.Converters
{
    [ValueConversion(typeof(SaveResult), typeof(bool?))]
    public class SaveResultToStatusConverter : SimpleValueConverter<SaveResult, bool?>
    {
        protected override bool? Convert(SaveResult value) => value?.Status;
    }
}