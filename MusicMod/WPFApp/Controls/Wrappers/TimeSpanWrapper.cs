using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Utils;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Wrappers
{
    internal class TimeSpanWrapper : ControlWrapper<TimeSpan, TextBox>
    {
        private static readonly Regex withHours = new(@"^(?<neg>-)?(?<h>\d+):(?<m>\d\d):(?<s>\d\d)(\.(?<ms>\d{1,3}))?$");

        private static readonly Regex withMinutes = new(@"^(?<neg>-)?(?<m>\d?\d):(?<s>\d\d)(\.(?<ms>\d{1,3}))?$");

        private static readonly Regex withSeconds = new(@"^(?<neg>-)?(?<s>\d?\d)(\.(?<ms>\d{1,3}))?s$");

        public TimeSpanWrapper() => UIElement.TextChanged += (s, e) => NotifyValueChanged();

        public override string ValueString => UIElement.Text;

        public override TextBox UIElement { get; } = new TextBox() { VerticalAlignment = VerticalAlignment.Center, MinWidth = 30 };

        protected override void setValue(TimeSpan value) => UIElement.Text = value.ToCompactString();

        protected override SaveResult<TimeSpan> tryGetValue(GetValueRequest request)
        {
            static int ParseInt(string s)
            {
                return s.Length == 0 ? 0 : int.Parse(s, System.Globalization.NumberStyles.Integer);
            }

            string text = UIElement.Text;
            int hours, minutes, seconds, milliseconds, sign;

            Match match;

            bool SetVariables()
            {
                match = withHours.Match(text);

                if (match.Success)
                {
                    return true;
                }

                match = withMinutes.Match(text);

                if (match.Success)
                {
                    return true;
                }

                match = withSeconds.Match(text);
                return match.Success;
            }

            if (!SetVariables())
            {
                return new(false);
            }

            hours = ParseInt(match.Groups["h"].Value);
            minutes = ParseInt(match.Groups["m"].Value);
            seconds = ParseInt(match.Groups["s"].Value);
            milliseconds = ParseInt(match.Groups["ms"].Value.PadRight(3, '0'));

            sign = match.Groups["neg"].Success ? -1 : 1;
            return new(sign * new TimeSpan(0, hours, minutes, seconds, milliseconds));
        }
    }
}