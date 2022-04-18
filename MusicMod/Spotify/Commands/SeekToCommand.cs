using System;
using System.Xml.Linq;
using Utils;

namespace Spotify.Commands
{
	public class SeekToCommand : Command
	{
		internal SeekToCommand(XElement element)
		{
			At = TimeSpan.FromMilliseconds(int.Parse(element.Attribute(nameof(Milliseconds)).Value));
		}

		public int Milliseconds => (int)At.TotalMilliseconds;

		public TimeSpan At { get; set; }

		private SeekToCommand(TimeSpan at) => At = at;

		public static SeekToCommand AtSeconds(int s) => new SeekToCommand(TimeSpan.FromSeconds(s));

		public static SeekToCommand AtMilliseconds(int ms) => new SeekToCommand(TimeSpan.FromMilliseconds(ms));

		protected override void AddDetail(XElement element)
		{
			element.SetAttributeValue("Milliseconds", Milliseconds);
		}

		public override string ToString()
		{
			return base.ToString() + $"({At.ToCompactString()})";
		}
	}
}