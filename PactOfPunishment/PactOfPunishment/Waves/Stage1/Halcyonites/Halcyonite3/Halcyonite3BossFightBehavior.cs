using HG;
using RoR2;

namespace PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite3
{
    public class Halcyonite3BossFightBehavior : Stage1HalcyoniteBossFightBehavior
    {
        protected override void OnMainBossSpawnedServer(CharacterBody body)
        {
            var halcyoniteBodyBehavior = body.EnsureComponent<Halcyonite3BodyBehavior>();
            halcyoniteBodyBehavior.rng = new Xoroshiro128Plus(this.CombatDirector.rng.nextUlong);
            halcyoniteBodyBehavior.BossStateMachine.SetState(new Halcyonite3States.Phase1());
        }
    }
}