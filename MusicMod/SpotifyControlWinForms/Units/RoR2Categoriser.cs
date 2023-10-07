using Music;
using MyRoR2;
using RuleExamples;
using RuleExamples.RiskOfRain2;
using Rules.RuleTypes.Interfaces;
using Utils;

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

    internal class RoR2VolumeController : Unit<RoR2Context, int>
    {
        private readonly VolumeController volumeController = new();

        private RoR2VolumeController(string name) : base(name)
        {
        }

        public static RoR2VolumeController Instance { get; } = new(nameof(RoR2VolumeController));

        protected override int Transform(RoR2Context input)
        {
            volumeController.GetOrAdd(nameof(RoR2Context.IsChargingTeleporter)).Volume = input.TeleporterState switch
            {
                TeleporterState.IdleToCharging or TeleporterState.Charging => input.IsChargingTeleporter ? 1 : 0.8,
                TeleporterState.Charged => 0.85,
                _ => input.SceneType == SceneType.Stage ? 0.7 : 0.85,
            };

            return volumeController.VolumePercent;
        }
    }
}