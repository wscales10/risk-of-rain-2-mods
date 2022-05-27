using System;
using System.Text.RegularExpressions;

namespace Utils
{
    public static class TimeSpanMethods
    {
        private static readonly Regex withHours = new Regex(@"^(?<neg>-)?(?<h>\d+):(?<m>\d\d):(?<s>\d\d)(\.(?<ms>\d{1,3}))?$");

        private static readonly Regex withMinutes = new Regex(@"^(?<neg>-)?(?<m>\d?\d):(?<s>\d\d)(\.(?<ms>\d{1,3}))?$");

        private static readonly Regex withSeconds = new Regex(@"^(?<neg>-)?(?<s>\d?\d)(\.(?<ms>\d{1,3}))?s$");

        public static string ToCompactString(this TimeSpan timeSpan)
        {
            string firstPart = timeSpan < TimeSpan.Zero ? "-" : string.Empty;
            string secondPart = timeSpan.Milliseconds != 0 ? timeSpan.ToString(@"\.FFF") : string.Empty;

            if (timeSpan.Hours != 0)
            {
                firstPart += timeSpan.ToString(@"h\:mm\:ss");
            }
            else if (timeSpan.Minutes != 0)
            {
                firstPart += timeSpan.ToString(@"mm\:ss");
            }
            else
            {
                firstPart += timeSpan.ToString("%s");
                secondPart += "s";
            }

            return firstPart + secondPart;
        }

        public static bool TryParseCompactString(string inputString, out TimeSpan timeSpan)
        {
            int ParseInt(string s)
            {
                return s.Length == 0 ? 0 : int.Parse(s, System.Globalization.NumberStyles.Integer);
            }

            int hours, minutes, seconds, milliseconds;
            bool shouldBeNegated;

            Match match;

            bool SetVariables()
            {
                match = withHours.Match(inputString);

                if (match.Success)
                {
                    return true;
                }

                match = withMinutes.Match(inputString);

                if (match.Success)
                {
                    return true;
                }

                match = withSeconds.Match(inputString);
                return match.Success;
            }

            if (!SetVariables())
            {
                timeSpan = default;
                return false;
            }

            hours = ParseInt(match.Groups["h"].Value);
            minutes = ParseInt(match.Groups["m"].Value);
            seconds = ParseInt(match.Groups["s"].Value);
            milliseconds = ParseInt(match.Groups["ms"].Value.PadRight(3, '0'));
            shouldBeNegated = match.Groups["neg"].Success;

            timeSpan = new TimeSpan(0, hours, minutes, seconds, milliseconds);

            if (shouldBeNegated)
            {
                timeSpan = timeSpan.Negate();
            }

            return true;
        }
    }
}