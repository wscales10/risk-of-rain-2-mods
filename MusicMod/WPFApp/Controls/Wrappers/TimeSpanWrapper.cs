using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Utils;

namespace WPFApp.Controls.Wrappers
{
	internal class TimeSpanWrapper : ControlWrapper<TimeSpan, TextBox>
	{
		private static readonly Regex withHours = new(@"^(?<neg>-)?(?<h>\d+):(?<m>\d\d):(?<s>\d\d)(\.(?<ms>\d{1,3}))?$");

		private static readonly Regex withMinutes = new(@"^(?<neg>-)?(?<m>\d?\d):(?<s>\d\d)(\.(?<ms>\d{1,3}))?$");

		private static readonly Regex withSeconds = new(@"^(?<neg>-)?(?<s>\d?\d)(\.(?<ms>\d{1,3}))?s$");

		public override TextBox UIElement { get; } = new TextBox() { VerticalAlignment = VerticalAlignment.Center };

		protected override void setValue(TimeSpan value) => UIElement.Text = value.ToCompactString();

		protected override void SetStatus(bool status) => UIElement.BorderBrush = status ? Brushes.DarkGray : Brushes.Red;

		protected override bool tryGetValue(out TimeSpan value)
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
				value = default;
				return false;
			}

			hours = ParseInt(match.Groups["h"].Value);
			minutes = ParseInt(match.Groups["m"].Value);
			seconds = ParseInt(match.Groups["s"].Value);
			milliseconds = ParseInt(match.Groups["ms"].Value.PadRight(3, '0'));

			sign = match.Groups["neg"].Success ? -1 : 1;

			value = sign * new TimeSpan(0, hours, minutes, seconds, milliseconds);
			return true;
		}
	}
}