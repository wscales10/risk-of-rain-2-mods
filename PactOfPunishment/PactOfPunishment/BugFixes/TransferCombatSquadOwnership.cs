namespace PactOfPunishment.BugFixes
{
    public class TransferCombatSquadOwnership : Module
    {
        public override void Init()
        {
            On.RoR2.BodySplitter.PerformInternal += this.BodySplitter_PerformInternal;
        }

        private void BodySplitter_PerformInternal(On.RoR2.BodySplitter.orig_PerformInternal orig, RoR2.BodySplitter self, RoR2.MasterSummon masterSummon)
        {
            var bodyObject = self.body?.gameObject;

            if (bodyObject)
            {
                masterSummon.summonerBodyObject = bodyObject;
            }
            else
            {
                this.Logger.LogWarning("BodySplitter has no suitable summoner body object");
            }

            orig(self, masterSummon);
        }
    }
}