using EntityStates;
using PactOfPunishment.Waves.Common;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PactOfPunishment.Waves.Stage2.Summoner
{
    public static class SummonerStates
    {
        public abstract class SummonerBaseState : EntityState
        {
            public SummonerReferences References;

            public virtual void OnBossSpawnedServer(SpawnCard.SpawnResult result, CharacterBody body)
            {
            }

            public override void ModifyNextState(EntityState nextState)
            {
                base.ModifyNextState(nextState);

                if (nextState is SummonerBaseState summonerState)
                {
                    summonerState.References = this.References;
                }
            }

            protected abstract SummonerBaseState? GetNextState();
        }

        public class Phase1 : PhaseState
        {
            public override float PhaseEndHealthThreshold => 2f / 3;

            protected override SummonerBaseState? GetNextState() => new FirstInterlude();
        }

        public class Phase2 : PhaseState // TODO: upgrade boss in later phases
        {
            public override float PhaseEndHealthThreshold => 1f / 3;

            protected override SummonerBaseState? GetNextState() => new SecondInterlude();
        }

        public class Phase3 : PhaseState
        {
            public override float PhaseEndHealthThreshold => 0;

            public override void OnEnter()
            {
                base.OnEnter();
                Utils.MakeBodyMortal(this.characterBody);
            }

            protected override SummonerBaseState? GetNextState() => null;
        }

        public class FirstInterlude : InterludeState
        {
            public override void OnEnter()
            {
                this.SupportToSpawn = 3;
                base.OnEnter();
            }

            protected override SummonerBaseState? GetNextState() => new Phase2();
        }

        public class SecondInterlude : InterludeState
        {
            public override void OnEnter()
            {
                this.SupportToSpawn = 6;
                base.OnEnter();
            }

            protected override SummonerBaseState? GetNextState() => new Phase3();
        }

        public abstract class PhaseState : SummonerBaseState, IOnBossTakeDamageReceiver
        {
            public abstract float PhaseEndHealthThreshold { get; }

            public override void ModifyNextState(EntityState nextState)
            {
                base.ModifyNextState(nextState);

                if (nextState is InterludeState interludeState)
                {
                    interludeState.PhaseStartingHealthFraction = this.PhaseEndHealthThreshold;
                }
            }

            void IOnBossTakeDamageReceiver.OnBossDamageTaken() => this.TryAdvanceState();

            private void TryAdvanceState()
            {
                // TODO: what if dead?

                if (this.healthComponent.combinedHealthFraction <= this.PhaseEndHealthThreshold && this.GetNextState() is SummonerBaseState state)
                {
                    this.outer.SetState(state);
                }
            }
        }

        public abstract class InterludeState : SummonerBaseState
        {
            private static readonly GameObject shieldRemovalEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/goldshores/GoldshoresArmorRemoval.prefab").WaitForCompletion();

            private readonly Dictionary<DirectorCard, EliteDef?> supportMonsterEliteDefs = new Dictionary<DirectorCard, EliteDef?>();

            private CombatSquad? combatSquad;

            public float PhaseStartingHealthFraction { get; set; }

            public int SupportToSpawn { get; protected set; }

            public override void OnEnter()
            {
                base.OnEnter();
                var combatSquadObject = new GameObject();
                combatSquadObject.transform.parent = this.transform;
                this.combatSquad = combatSquadObject.AddComponent<CombatSquad>();
                this.combatSquad.onDefeatedServer += this.CombatSquad_onDefeatedServer;
                this.SetupMainBossBody(this.characterBody);
                var timer = this.gameObject.AddComponent<DoSomethingAtFixedRate>();
                timer.interval = 0.5f;
                timer.doSomething = this.TrySpawnSupport;
            }

            public override void OnExit()
            {
                if (this.combatSquad)
                {
                    Object.Destroy(this.combatSquad!.gameObject);
                }

                if (this.characterBody)
                {
                    this.RemoveImmunity(this.characterBody!);
                }

                base.OnExit();
            }

            public override void OnBossSpawnedServer(SpawnCard.SpawnResult result, CharacterBody body)
            {
                base.OnBossSpawnedServer(result, body);
                int directorCreditCost = result.spawnRequest.spawnCard.directorCreditCost;
                float healthMultiplier = this.References.GetComponent<InfiniteTowerWaveController>().totalWaveCredits * 0.15f / directorCreditCost;
                float supportMonsterMaxHealth = body.healthComponent.fullCombinedHealth;
                float? bossMaxHealth = this.healthComponent.fullCombinedHealth;

                if (supportMonsterMaxHealth > 0 && bossMaxHealth != null)
                {
                    healthMultiplier = Mathf.Min(healthMultiplier, bossMaxHealth.Value * 0.15f / supportMonsterMaxHealth);
                }

                Debug.Log($"Scaling health for {body.name} by {healthMultiplier}");
                var summonerBehavior = this.References.GetComponent<SummonerBossFightBehavior>();
                body.ScaleMaxHealth(summonerBehavior, healthMultiplier);
                this.combatSquad!.AddMember(body.master);
            }

            private DirectorCard SelectSupportDirectorCard(CombatDirector combatDirector)
            {
                return combatDirector.rng.NextElementUniform(this.References.SupportMonsterDirectorCards);
            }

            private void RemoveImmunity(CharacterBody body)
            {
                EffectManager.SpawnEffect(shieldRemovalEffectPrefab, new EffectData
                {
                    origin = body.coreTransform.position
                }, transmit: true);
                body.RemoveBuff(RoR2Content.Buffs.Immune);
            }

            private void TrySpawnSupport()
            {
                if (this.SupportToSpawn <= 0)
                {
                    return;
                }

                var combatDirector = this.References.GetComponent<CombatDirector>();
                DirectorCard directorCard = this.SelectSupportDirectorCard(combatDirector);

                // For Artifact of Honor support
                if (this.supportMonsterEliteDefs.TryGetValue(directorCard, out EliteDef? eliteDef))
                {
                    this.supportMonsterEliteDefs[directorCard] = eliteDef = combatDirector.currentActiveEliteTier.GetRandomAvailableEliteDef(combatDirector.rng);
                }

                this.References.MainBossBodyCurrentlySummoningSupport = this.characterBody;
                try
                {
                    if (combatDirector.Spawn(directorCard.spawnCard, eliteDef, this.References.GetComponent<InfiniteTowerWaveController>().spawnTarget.transform, directorCard.spawnDistance, directorCard.preventOverhead))
                    {
                        this.SupportToSpawn--;
                    }
                }
                finally
                {
                    this.References.MainBossBodyCurrentlySummoningSupport = null;
                }
            }

            private void SetupMainBossBody(CharacterBody? body)
            {
                if (body is null)
                {
                    return;
                }

                body.AddBuff(RoR2Content.Buffs.Immune);
                CleanseSystem.CleanseBodyServer(body, true, false, false, true, false, false);
                Utils.DirectHeal(this.healthComponent, this.PhaseStartingHealthFraction);
            }

            private void CombatSquad_onDefeatedServer()
            {
                this.outer.SetState(this.GetNextState());
            }
        }
    }
}