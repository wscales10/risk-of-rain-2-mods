using System.Xml.Linq;
using Utils;

namespace Spotify.Commands
{
	public enum RepeatMode
	{
		Off,

		Track,

		Context
	}

	public class SetPlaybackOptionsCommand : Command
	{
		public SetPlaybackOptionsCommand()
		{
		}

		internal SetPlaybackOptionsCommand(XElement element)
		{
			RepeatMode = element.Attribute(nameof(RepeatMode))?.Value.AsEnum<RepeatMode>();
			var shuffle = element.Attribute(nameof(Shuffle))?.Value;
			Shuffle = shuffle is null ? (bool?)null : bool.Parse(shuffle);
			var volumePercent = element.Attribute(nameof(VolumePercent))?.Value;
			VolumePercent = volumePercent is null ? (int?)null : int.Parse(volumePercent);
		}

		public RepeatMode? RepeatMode { get; set; }

		public bool? Shuffle { get; set; }

		public int? VolumePercent { get; set; }

		protected override void AddDetail(XElement element)
		{
			if (!(RepeatMode is null))
				element.SetAttributeValue(nameof(RepeatMode), RepeatMode);
			if (!(Shuffle is null))
				element.SetAttributeValue(nameof(Shuffle), Shuffle);
			if (!(VolumePercent is null))
				element.SetAttributeValue(nameof(VolumePercent), VolumePercent);
		}
	}
}