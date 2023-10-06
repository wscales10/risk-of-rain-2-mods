using MyRoR2;
using RuleExamples;
using RuleExamples.RiskOfRain2;
using Rules.RuleTypes.Interfaces;
using Spotify.Commands;

namespace SpotifyControlWinForms.Units
{
    internal class RoR2Categoriser : RuleUnit<RoR2Context, string>
    {
        private RoR2Categoriser(string name) : base(name, RuleParsers.RoR2ToString)
        {
        }

        public static RoR2Categoriser Instance { get; } = new(nameof(RoR2Categoriser));

        public override IRule<RoR2Context, string> DefaultRule => Ror2Rule.Instance;
    }

    internal class RoR2VolumeController : Unit<RoR2Context, ICommandList>
    {
        private RoR2VolumeController(string name) : base(name)
        {
        }

        public static RoR2VolumeController Instance { get; } = new(nameof(RoR2VolumeController));

        protected override ICommandList Transform(RoR2Context input)
        {
            return new CommandList(new SetVolumeCommand()
            {
                VolumeControlName = nameof(RoR2Context.IsChargingTeleporter),
                VolumePercent = input.TeleporterState != TeleporterState.Charging || input.IsChargingTeleporter ? 100 : 80
            });
        }
    }
}