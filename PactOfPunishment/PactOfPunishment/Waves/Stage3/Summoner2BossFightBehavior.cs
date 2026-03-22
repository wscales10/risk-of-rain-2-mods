using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using RoR2;
using System.Linq;

namespace PactOfPunishment.Waves.Stage3
{
    public class Summoner2BossFightBehavior : PortableMiniBossFightBehavior<Summoner2BossFightBehavior>
    {
        public bool disableTeleport = true;

        internal static AssetPromise<CharacterSpawnCard> eggSpawnCard;

        internal static AssetPromise<CharacterSpawnCard> parentSpawnCard;

        private DoSomethingAtFixedRate? eggSpawner;

        private EliteDef[] eliteDefs;

        public override void Awake()
        {
            base.Awake();

            this.eliteDefs = this.CombatDirector.GetEliteDefs(parentSpawnCard.Value).ToArray(); // TODO: this should be more sophisticated

            this.eggSpawner = this.gameObject.AddComponent<DoSomethingAtFixedRate>();
            this.eggSpawner.interval = 3;
            this.eggSpawner.doSomething = this.SpawnEgg;
            this.ApplyEnabledState();
        }

        protected override void OnCombatSquadMemberDiscovered(CharacterBody body)
        {
            base.OnCombatSquadMemberDiscovered(body);

            if (body.Is(RoR2Content.BodyPrefabs.ParentBody))
            {
                body.ScaleDifficultyAsBoss(158, 158, false, false); // TODO: scale more?
                Utils.MakeScaledElite(body, this.CombatDirector.rng.NextElementUniform(this.eliteDefs));

                // TODO: drop egg on death?
            }
        }

        public override void ApplyEnabledState()
        {
            base.ApplyEnabledState();
            if (this.eggSpawner)
            {
                this.eggSpawner!.enabled = this.CustomEnabled;
            }
        }

        private void SpawnEgg()
        {
            if (!this.CustomEnabled)
            {
                return;
            }

            var spawnTarget = this.EncounterContext.SpawnTarget;

            if (!spawnTarget)
            {
                return;
            }

            this.CombatDirector.Spawn(eggSpawnCard.Value, null, spawnTarget.transform, DirectorCore.MonsterSpawnDistance.Standard, false);
        }
    }
}