namespace PactOfPunishment
{
    public class ReduceAccidentalVoidFogDamageToPlayer : Module
    {
        public override void Init()
        {
            On.EntityStates.InfiniteTowerSafeWard.AwaitingActivation.OnEnter += this.AwaitingActivation_OnEnter;
            On.EntityStates.InfiniteTowerSafeWard.Burrow.OnEnter += this.Burrow_OnEnter;
            On.EntityStates.InfiniteTowerSafeWard.Travelling.OnEnter += this.Travelling_OnEnter;
        }

        private void AwaitingActivation_OnEnter(On.EntityStates.InfiniteTowerSafeWard.AwaitingActivation.orig_OnEnter orig, EntityStates.InfiniteTowerSafeWard.AwaitingActivation self)
        {
            AdjustZoneRadius(ref self.radius);
            orig(self);
        }

        private void Burrow_OnEnter(On.EntityStates.InfiniteTowerSafeWard.Burrow.orig_OnEnter orig, EntityStates.InfiniteTowerSafeWard.Burrow self)
        {
            AdjustZoneRadius(ref self.radius);
            orig(self);
        }

        private void Travelling_OnEnter(On.EntityStates.InfiniteTowerSafeWard.Travelling.orig_OnEnter orig, EntityStates.InfiniteTowerSafeWard.Travelling self)
        {
            AdjustZoneRadius(ref self.radius);
            orig(self);
        }

        private static void AdjustZoneRadius(ref float radius)
        {
            radius *= 4 / 3f;
        }
    }
}
