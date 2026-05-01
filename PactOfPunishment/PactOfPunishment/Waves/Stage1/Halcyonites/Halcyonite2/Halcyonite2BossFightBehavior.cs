using HG;
using RoR2;

namespace PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite2
{
    public class Halcyonite2BossFightBehavior : Stage1HalcyoniteBossFightBehavior
    {
        public override void Awake()
        {
            base.Awake();
            this.gameObject.EliminateCombatSquadWhenLastMainMemberDies(this.CombatDirector.combatSquad, x => x.GetBody()?.bodyIndex == DLC2Content.BodyPrefabs.HalcyoniteBody.bodyIndex, callback: () => this.CombatDirector.enabled = false);
        }

        protected override void OnMainBossSpawnedServer(CharacterBody body)
        {
            var halcyoniteBodyBehavior = body.EnsureComponent<Halcyonite2BodyBehavior>();
            halcyoniteBodyBehavior.CombatDirector = this.CombatDirector;
            halcyoniteBodyBehavior.BossStateMachine.SetState(new Halcyonite2States.Phase1());
        }

        protected override void OnAddSpawnedServer(CharacterBody body)
        {
            base.OnAddSpawnedServer(body);

            if (body.Is(DLC3Content.BodyPrefabs.WorkerUnitBody))
            {
                body.ScaleDifficultyAsBoss(new BossScalingArgs1(1f, 30f, false, 10), false); // TODO: coef was kinda factored into spawning
            }
        }
    }
}