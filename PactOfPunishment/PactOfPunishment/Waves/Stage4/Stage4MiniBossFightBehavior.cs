using HG;
using PactOfPunishment.Waves.Common;
using RoR2;

namespace PactOfPunishment.Waves.Stage4
{
    public class Stage4MiniBossFightBehavior : BossFightBehavior
    {
        internal bool haveMainBossesSpawned = false;

        public override void OnEnable()
        {
            base.OnEnable();
            InfiniteTowerExplicitSpawnWaveController.OnExplicitWaveInitialize += this.InfiniteTowerExplicitSpawnWaveController_OnExplicitWaveInitialize;
        }

        public override void OnDisable()
        {
            InfiniteTowerExplicitSpawnWaveController.OnExplicitWaveInitialize -= this.InfiniteTowerExplicitSpawnWaveController_OnExplicitWaveInitialize;
            base.OnDisable();
        }

        protected override void OnBossSpawnedServer(CharacterBody body)
        {
            if (this.haveMainBossesSpawned)
            {
                this.OnAddSpawnedServer(body);
            }
            else
            {
                this.OnMainBossSpawnedServer(body);
            }
        }

        private void InfiniteTowerExplicitSpawnWaveController_OnExplicitWaveInitialize()
        {
            this.haveMainBossesSpawned = true;
        }

        private void OnMainBossSpawnedServer(CharacterBody body)
        {
            body.master.ScaleDifficultyAsBoss();
            body.DisableStunsEtc();
            body.ResistNonTargetedDamage();

            if (body.Is(DLC3Content.BodyPrefabs.DefectiveUnitBody))
            {
                body.EnsureComponent<Invalidator.BodyBehavior>();
            }
            else if (body.Is(RoR2Content.BodyPrefabs.LemurianBruiserBody))
            {
                body.EnsureComponent<BlazingElderLemurian.BodyBehavior>();
            }
            else if (body.Is(DLC1Content.BodyPrefabs.GupBody))
            {
                body.EnsureComponent<Gup.BodyBehavior>();
            }
            else if (body.Is(RoR2Content.BodyPrefabs.TitanGoldBody))
            {
                body.EnsureComponent<Aurelionite.BodyBehavior>();
            }
        }

        private void OnAddSpawnedServer(CharacterBody body)
        {
            if (body.Is(DLC1Content.BodyPrefabs.AcidLarvaBody))
            {
                body.MakeUnscaledEliteUsingEquipment(RoR2Content.Elites.Fire);
                body.ScaleMaxHealth(this, 0.4f);
                body.ScaleDamage(this, 0.2f);
            }
            else if (body.Is(DLC1Content.BodyPrefabs.GeepBody))
            {
                body.MakeUnscaledEliteUsingEquipment(RoR2Content.Elites.Fire);
            }
            else if (body.Is(RoR2Content.BodyPrefabs.LemurianBody))
            {
                body.MakeUnscaledEliteUsingBuff(RoR2Content.Elites.Fire);
            }
        }
    }
}