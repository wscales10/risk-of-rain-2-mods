using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Utils;

namespace WPFApp.Controls.Wrappers
{
	internal class TimeSpanWrapper : ControlWrapper<TimeSpan, TextBox>
	{
		private static readonly Regex withHours = new(@"^(?<neg>-)?(?<h>\d+):(?<m>\d\d):(?<s>\d\d)(\.(?<ms>\d{1,3}))?$");

		private static readonly Regex withMinutes = new(@"^(?<neg>-)?(?<m>\d?\d):(?<s>\d\d)(\.(?<ms>\d{1,3}))?$");

		private static readonly Regex withSeconds = new(@"^(?<neg>-)?(?<s>\d?\d)(\.(?<ms>\d{1,3}))?s$");

		public override string ValueString => UIElement.Text;

        public TimeSpanWrapper() => UIElement.TextChanged += (s, e) => NotifyValueChanged();

		public override TextBox UIElement { get; } = new TextBox() { VerticalAlignment = VerticalAlignment.Center, MinWidth = 30 };

		protected override void setValue(TimeSpan value) => UIElement.Text = value.ToCompactString();

		protected override SaveResult<TimeSpan> tryGetValue(bool trySave)
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
				return (match = withHours.Match(text)).Success || (match = withMinutes.Match(text)).Success || (match = withSeconds.Match(text)).Success;
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