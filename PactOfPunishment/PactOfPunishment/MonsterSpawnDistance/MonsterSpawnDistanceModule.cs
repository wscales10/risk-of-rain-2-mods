namespace PactOfPunishment.MonsterSpawnDistance
{
    public class MonsterSpawnDistanceModule : Module
    {
        public override void Init()
        {
            On.RoR2.DirectorCore.GetMonsterSpawnDistance += this.DirectorCore_GetMonsterSpawnDistance;
        }

        private void DirectorCore_GetMonsterSpawnDistance(On.RoR2.DirectorCore.orig_GetMonsterSpawnDistance orig, RoR2.DirectorCore.MonsterSpawnDistance input, out float minimumDistance, out float maximumDistance)
        {
            orig(input, out minimumDistance, out maximumDistance);
            MonsterSpawnDistanceApi.TryGetCustomMonsterSpawnDistance(input, ref minimumDistance, ref maximumDistance);
        }
    }
}
