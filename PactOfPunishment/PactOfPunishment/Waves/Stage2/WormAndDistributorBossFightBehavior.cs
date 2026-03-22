using PactOfPunishment.Waves.Common;
using RoR2;

namespace PactOfPunishment.Waves.Stage2
{
    public class WormAndDistributorBossFightBehavior : PortableMiniBossFightBehavior<WormAndDistributorBossFightBehavior>
    {
        public bool disableSecondarySkills = true;

        public override void Awake()
        {
            base.Awake();
            this.gameObject.EliminateCombatSquadWhenLastMainMemberDies(this.CombatDirector.combatSquad, x => x.GetBody().IsOneOf(DLC2Content.BodyPrefabs.ScorchlingBody, DLC3Content.BodyPrefabs.MinePodBody), x => x.GetBody().Is(DLC3Content.BodyPrefabs.SolusMineBody));
        }
    }
}