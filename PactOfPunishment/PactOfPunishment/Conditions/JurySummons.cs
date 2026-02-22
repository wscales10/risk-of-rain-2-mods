using HG;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PactOfPunishment.Conditions
{
    public sealed partial class JurySummons : DefaultConditionDef
    {
        public override int MaxRank => 3;

        public override void Init()
        {
            On.RoR2.InfiniteTowerRun.Start += this.InfiniteTowerRun_Start;
            On.RoR2.InfiniteTowerWaveController.OnEnable += InfiniteTowerWaveController_OnEnable;
            ScaleMaxSquadCount.OnScaleMaxSquadCount += ScaleMaxSquadCountForJurySummons;
        }

        private static void InfiniteTowerWaveController_OnEnable(On.RoR2.InfiniteTowerWaveController.orig_OnEnable orig, InfiniteTowerWaveController self)
        {
            if (!self.isBossWave && Run.instance.TryGetComponent<JurySummonsBehavior>(out var behavior))
            {
                behavior.AddCombatDirector(self.combatDirector);
            }

            orig(self);
        }

        private static void ScaleMaxSquadCountForJurySummons(CombatDirector combatDirector, ref uint maxSquadCount)
        {
            if (combatDirector.TryGetComponent<JurySummonsBehavior>(out var behavior))
            {
                maxSquadCount = (uint)Mathf.CeilToInt(maxSquadCount * (1 + behavior.spawnRateBonus));
            }
        }

        private void InfiniteTowerRun_Start(On.RoR2.InfiniteTowerRun.orig_Start orig, InfiniteTowerRun self)
        {
            var behavior = self.EnsureComponent<JurySummonsBehavior>();
            behavior.enabled = this.IsEnabled(self);
            behavior.spawnRateBonus = this.GetRank(self) * 0.2f;
            orig(self);
        }

        public class JurySummonsBehavior : MonoBehaviour
        {
            public float savedCredits;

            public float spawnRateBonus;

            private readonly HashSet<CombatDirector> combatDirectors = new HashSet<CombatDirector>();

            private bool isSpawningExtraCopies;

            public void AbsorbCost(float cost)
            {
                this.savedCredits += cost * this.spawnRateBonus;
            }

            public void AddCombatDirector(CombatDirector? combatDirector) // What if the same combat director is used for non-boss and boss waves?
            {
                if (!combatDirector)
                {
                    Debug.LogError($"{nameof(combatDirector)} is null");
                    return;
                }

                if (combatDirector!.onSpawnedServer is null)
                {
                    Debug.LogError($"{nameof(combatDirector.onSpawnedServer)} is null");
                    return;
                }

                if (this.combatDirectors.Add(combatDirector))
                {
                    MonsterTracker.TrackCombatDirector(combatDirector);
                }
            }

            public void OnEnable()
            {
                SpawnCard.onSpawnedServerGlobal += this.OnSpawnedServerGlobal;
            }

            public void OnDisable()
            {
                SpawnCard.onSpawnedServerGlobal -= this.OnSpawnedServerGlobal;
            }

            private static DeathRewards? GetDeathRewards(GameObject spawnedEntity, Func<CharacterMaster, bool>? masterPredicate = null)
            {
                if (spawnedEntity && spawnedEntity.TryGetComponent<CharacterMaster>(out var spawnedMaster) && (masterPredicate?.Invoke(spawnedMaster) ?? true))
                {
                    var bodyObject = spawnedMaster.GetBodyObject();

                    if (bodyObject && bodyObject.TryGetComponent<DeathRewards>(out var deathRewards))
                    {
                        return deathRewards;
                    }
                }

                return null;
            }

            private void OnSpawnedServerGlobal(SpawnCard.SpawnResult spawnResult)
            {
                if (!spawnResult.success)
                {
                    return;
                }

                var deathRewards = GetDeathRewards(spawnResult.spawnedInstance, spawnedMaster => spawnedMaster.TryGetComponent<MonsterTracker>(out var tracker) && tracker.combatDirector && this.combatDirectors.Contains(tracker.combatDirector!));

                if (deathRewards)
                {
                    var originalSpawnValue = deathRewards!.spawnValue;
                    this.ScaleDeathRewards(deathRewards);
                    this.AbsorbCost(originalSpawnValue);
                    this.TrySpawnExtraCopies(spawnResult.spawnRequest, originalSpawnValue);
                }
            }

            private void ScaleDeathRewards(DeathRewards deathRewards)
            {
                Utils.ScaleDeathRewards(deathRewards, 1 / (1f + this.spawnRateBonus));
            }

            private void TrySpawnExtraCopies(DirectorSpawnRequest spawnRequest, float cost)
            {
                if (this.isSpawningExtraCopies)
                {
                    return;
                }

                int numberOfExtraCopiesToSpawn = Mathf.FloorToInt(this.savedCredits / cost);

                for (int i = 0; i < numberOfExtraCopiesToSpawn; i++)
                {
                    this.isSpawningExtraCopies = true;

                    try
                    {
                        var spawnedEntity = DirectorCore.instance.TrySpawnObject(spawnRequest);
                        var deathRewards = GetDeathRewards(spawnedEntity);

                        if (deathRewards)
                        {
                            this.ScaleDeathRewards(deathRewards!);
                        }

                        this.savedCredits -= cost;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }

                    this.isSpawningExtraCopies = false;
                }
            }
        }
    }
}