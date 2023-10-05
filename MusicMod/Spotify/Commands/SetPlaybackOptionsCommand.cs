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
        }

        public RepeatMode? RepeatMode { get; set; }

        public bool? Shuffle { get; set; }

        protected override void AddDetail(XElement element)
        {
            if (!(RepeatMode is null))
                element.SetAttributeValue(nameof(RepeatMode), RepeatMode);
            if (!(Shuffle is null))
                element.SetAttributeValue(nameof(Shuffle), Shuffle);
        }
    }

    public class SetVolumeCommand : Command
    {
        public SetVolumeCommand()
        {
        }

        internal SetVolumeCommand(XElement element)
        {
            var volumePercent = element.Attribute(nameof(VolumePercent)).Value;
            VolumePercent = int.Parse(volumePercent);
            VolumeControlName = (element.Attribute(nameof(VolumeControlName))?.Value);
        }

        public int VolumePercent { get; set; }

        public string VolumeControlName { get; set; }

        protected override void AddDetail(XElement element)
        {
            element.SetAttributeValue(nameof(VolumePercent), VolumePercent);

            if (VolumeControlName != null)
            {
                element.SetAttributeValue(nameof(VolumeControlName), VolumeControlName);
            }
        }
    }
}