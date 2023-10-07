using Spotify.Commands;

namespace SpotifyControlWinForms.Units
{
    internal class SpotifyVolumeController : Unit<int, ICommandList>
    {
        private SpotifyVolumeController(string name) : base(name)
        {
        }

        public static SpotifyVolumeController Instance { get; } = new(nameof(SpotifyVolumeController));

        protected override ICommandList Transform(int input)
        {
            return new CommandList(new SetVolumeCommand() { VolumePercent = input });
        }

        protected override void HandleInput(int input, IList<string> changedPropertyNames)
        {
            if (input != CachedInput)
            {
                base.HandleInput(input, changedPropertyNames);
            }
        }
    }
}