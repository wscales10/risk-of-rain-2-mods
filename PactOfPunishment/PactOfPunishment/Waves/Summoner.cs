using EntityStates;
using HG;
using PactOfPunishment.Conditions;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PactOfPunishment.Waves
{
    public partial class Summoner : MainBossWaveDefinition<InfiniteTowerBossWaveController>
    {
        protected override UpgradeWaveStrategy GetUpgradeMainBossStrategy()
        {
            return ScriptableObject.CreateInstance<SummonerUpgradeStrategy>();
        }

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerBossWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);
            SetupStateMachinePrefab(wavePrefab.gameObject.AddComponent<EntityStateMachine>());

            dir.maxSquadCount = 1;
            wavePrefab.immediateCreditsFraction = 0.45f;
            wavePrefab.guaranteeInitialChampion = true;

            // TODO: boss wave start UI is not accurate? same for all bosses?
            wavePrefab.gameObject.AddComponent<SummonerBehavior>();
        }

        private static void SetupStateMachinePrefab(EntityStateMachine x)
        {
            x.customName = nameof(Summoner);
            x.initialStateType = new SerializableEntityStateType(typeof(SummonerStates.PreFight));

            // x.mainStateType = new SerializableEntityStateType(typeof(SummonerStates.PreFight));
            x.commonComponents = new EntityStateMachine.CommonComponentCache(x.gameObject);

            // x.nextStateModifier = this.ModifyNextState;
        }

        public static class SummonerStates
        {
            public struct References
            {
                public CharacterBody? MainBossBody;

                public int MainBossMonsterIndex;

                public DirectorCard[] SupportMonsterDirectorCards;

                public bool ExpandFirstInterludeSupportSelection;
            }

            public abstract class SummonerBaseState : EntityState
            {
                public References References;

                public virtual void OnMainBossDamageTaken(HealthComponent healthComponent)
                {
                }

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
            }

            public class PreFight : SummonerBaseState
            {
                public override void OnEnter()
                {
                    Debug.Log("Entering PreFight");
                    base.OnEnter();
                    var combatDirector = this.GetComponent<CombatDirector>();
                    var selector = combatDirector.finalMonsterCardsSelection;
                    this.References.MainBossMonsterIndex = selector.EvaluateToChoiceIndex(combatDirector.rng.nextNormalizedFloat);
                    combatDirector.OverrideNextBossCard(selector.GetChoice(this.References.MainBossMonsterIndex).value, false);
                }

                public override void OnBossSpawnedServer(SpawnCard.SpawnResult result, CharacterBody body)
                {
                    base.OnBossSpawnedServer(result, body);
                    float healthMultiplier = 800f / result.spawnRequest.spawnCard.directorCreditCost; // TODO: is this correct?
                    Debug.Log($"Scaling health for {body.name} by {healthMultiplier}");
                    Utils.ApplyHealthMultiplier(body, healthMultiplier);
                    this.SetupBossHealthComponent(body.healthComponent);
                    this.References.MainBossBody = body; // TODO: what about swarms?
                    this.References.SupportMonsterDirectorCards = this.GetSupportMonsterDirectorCards(2);
                    var summonerBehavior = this.GetComponent<SummonerBehavior>();
                    this.References.ExpandFirstInterludeSupportSelection = summonerBehavior != null && summonerBehavior.ExpandFirstInterludeSupportSelection;
                    this.outer.SetState(new Phase1());
                }

                private static bool CanBeSupportMonster(WeightedSelection<DirectorCard>.ChoiceInfo choice)
                { // TODO: check all bosses' interactions with artifact of kin
                    return choice.value?.cost > 14 && choice.value?.spawnCard is CharacterSpawnCard characterSpawnCard && characterSpawnCard.prefab.GetComponent<CharacterMaster>()?.bodyPrefab?.GetComponent<CharacterBody>()?.isChampion == false;
                }

                private DirectorCard[] GetSupportMonsterDirectorCards(int count)
                {
                    var combatDirector = this.GetComponent<CombatDirector>();
                    var selector = combatDirector.finalMonsterCardsSelection;
                    var list = new List<DirectorCard>();
                    var ignored = new int[] { this.References.MainBossMonsterIndex }.Union(Enumerable.Range(0, selector.Count).Where(x => !CanBeSupportMonster(selector.GetChoice(x)))).ToArray();

                    for (int i = 0; i < count; i++)
                    {
                        var choiceIndex = selector.EvaluateToChoiceIndex(combatDirector.rng.nextNormalizedFloat, ignored);
                        list.Add(selector.GetChoice(choiceIndex).value);
                        ArrayUtils.ArrayAppend(ref ignored, choiceIndex);
                    }

                    return list.ToArray();
                }

                private void SetupBossHealthComponent(HealthComponent healthComponent)
                {
                    var markAllDamageNonLethalBehavior = healthComponent.EnsureComponent<MakeAllDamageNonLethalBehavior>();
                    markAllDamageNonLethalBehavior.enabled = true;
                    healthComponent.AddOnIncomingDamageServerReceiver(markAllDamageNonLethalBehavior);
                    var onTakeDamageServerReceiver = healthComponent.EnsureComponent<OnTakeDamageServerReceiver>();
                    onTakeDamageServerReceiver.stateMachine = this.outer;
                    healthComponent.AddOnTakeDamageServerReceiver(onTakeDamageServerReceiver);
                }
            }

            public class Phase1 : PhaseState
            {
                public override float PhaseEndHealthThreshold => 2f / 3;

                protected override SummonerBaseState GetNextState() => new FirstInterlude();
            }

            public class Phase2 : PhaseState // TODO: upgrade boss in later phases
            {
                public override float PhaseEndHealthThreshold => 1f / 3;

                protected override SummonerBaseState GetNextState() => new SecondInterlude();
            }

            public class Phase3 : PhaseState
            {
                public override float PhaseEndHealthThreshold => 0;

                public override void OnEnter()
                {
                    base.OnEnter();
                    MakeBodyMortal(this.References.MainBossBody);
                }

                protected override SummonerBaseState GetNextState() => new Death();

                private static void MakeBodyMortal(CharacterBody? body)
                {
                    if (!body)
                    {
                        return;
                    }

                    body!.healthComponent.gameObject.GetComponent<MakeAllDamageNonLethalBehavior>().enabled = false;
                }
            }

            public class Death : SummonerBaseState
            {
            }

            public class FirstInterlude : InterludeState
            {
                public override float PhaseStartingHealthFraction => 2f / 3;

                public override void OnEnter()
                {
                    this.SupportToSpawn = 3;
                    base.OnEnter();
                }

                protected override SummonerBaseState GetNextState() => new Phase2();

                protected override DirectorCard SelectSupportDirectorCard(CombatDirector combatDirector)
                {
                    if (this.References.ExpandFirstInterludeSupportSelection)
                    {
                        return combatDirector.rng.NextElementUniform(this.References.SupportMonsterDirectorCards);
                    }
                    else
                    {
                        return this.References.SupportMonsterDirectorCards[0];
                    }
                }
            }

            public class SecondInterlude : InterludeState
            {
                public override float PhaseStartingHealthFraction => 1f / 3;

                public override void OnEnter()
                {
                    this.SupportToSpawn = 6;
                    base.OnEnter();
                }

                protected override SummonerBaseState GetNextState() => new Phase3();

                protected override DirectorCard SelectSupportDirectorCard(CombatDirector combatDirector)
                {
                    return combatDirector.rng.NextElementUniform(this.References.SupportMonsterDirectorCards);
                }
            }

            public abstract class PhaseState : MainBossAliveState
            {
                public abstract float PhaseEndHealthThreshold { get; }

                public override void OnMainBossDamageTaken(HealthComponent healthComponent)
                {
                    base.OnMainBossDamageTaken(healthComponent);

                    // TODO: what if dead?

                    if (healthComponent.combinedHealthFraction <= this.PhaseEndHealthThreshold)
                    {
                        this.outer.SetState(this.GetNextState());
                    }
                }
            }

            public abstract class InterludeState : MainBossAliveState
            {
                private static readonly GameObject shieldRemovalEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/goldshores/GoldshoresArmorRemoval.prefab").WaitForCompletion();

                private CombatSquad? combatSquad;

                private GameObject? combatSquadObject;

                public abstract float PhaseStartingHealthFraction { get; } // TODO: DRY

                public int SupportToSpawn { get; protected set; }

                public override void OnEnter()
                {
                    base.OnEnter();
                    this.combatSquadObject = new GameObject();
                    this.combatSquadObject.transform.parent = this.transform;
                    this.combatSquad = this.combatSquadObject.AddComponent<CombatSquad>();
                    this.combatSquad.onDefeatedServer += this.CombatSquad_onDefeatedServer;
                    this.SetupMainBossBody(this.References.MainBossBody);
                    var timer = this.gameObject.AddComponent<PeriodicallyDoSomething>();
                    timer.interval = 0.5f;
                    timer.doSomething = this.TrySpawnSupport;
                }

                public override void OnExit()
                {
                    base.OnExit();
                    UnityEngine.Object.Destroy(this.combatSquadObject);
                    if (this.References.MainBossBody)
                    {
                        this.RemoveImmunity(this.References.MainBossBody!);
                    }
                }

                public override void OnBossSpawnedServer(SpawnCard.SpawnResult result, CharacterBody body)
                {
                    base.OnBossSpawnedServer(result, body);
                    int directorCreditCost = result.spawnRequest.spawnCard.directorCreditCost;
                    float healthMultiplier = 120f / directorCreditCost; // TODO: is this correct?
                    float myMaxHealth = body.healthComponent.fullCombinedHealth;
                    float? bossMaxHealth = this.References.MainBossBody?.healthComponent.fullCombinedHealth;

                    if (myMaxHealth > 0 && bossMaxHealth != null)
                    {
                        healthMultiplier = Mathf.Min(healthMultiplier, bossMaxHealth.Value * 0.15f / myMaxHealth);
                    }

                    Debug.Log($"Scaling health for {body.name} by {healthMultiplier}");
                    Utils.ApplyHealthMultiplier(body, healthMultiplier);
                    this.combatSquad!.AddMember(body.master);
                }

                protected abstract DirectorCard SelectSupportDirectorCard(CombatDirector combatDirector);

                private static void DirectHeal(HealthComponent healthComponent, float healthFraction)
                {
                    healthComponent.Networkhealth = healthComponent.fullCombinedHealth * Mathf.Clamp01(healthFraction); // TODO: Don't lower below current health?
                }

                private void RemoveImmunity(CharacterBody body)
                {
                    EffectManager.SpawnEffect(shieldRemovalEffectPrefab, new EffectData
                    {
                        origin = body.coreTransform.position
                    }, transmit: true);
                    body.RemoveBuff(RoR2Content.Buffs.Immune);
                }

                private void TrySpawnSupport() // TODO: limit to 4-5 different monsters?
                {
                    if (this.SupportToSpawn <= 0)
                    {
                        return;
                    }

                    var combatDirector = this.GetComponent<CombatDirector>();
                    var wave = this.GetComponent<InfiniteTowerWaveController>();
                    DirectorCard directorCard = this.SelectSupportDirectorCard(combatDirector);

                    if (combatDirector.Spawn(directorCard.spawnCard, null, wave.spawnTarget.transform, directorCard.spawnDistance, directorCard.preventOverhead))
                    {
                        this.SupportToSpawn--;
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
                    DirectHeal(body.healthComponent, this.PhaseStartingHealthFraction);
                }

                private void CombatSquad_onDefeatedServer()
                {
                    this.outer.SetState(this.GetNextState());
                }
            }

            public abstract class MainBossAliveState : SummonerBaseState
            {
                protected abstract SummonerBaseState GetNextState();
            }
        }

        public sealed class OnTakeDamageServerReceiver : MonoBehaviour, IOnTakeDamageServerReceiver
        {
            public EntityStateMachine stateMachine;

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (this.stateMachine.state is SummonerStates.SummonerBaseState summonerState)
                {
                    summonerState.OnMainBossDamageTaken(damageReport.victim);
                }
            }
        }

        public class SummonerBehavior : MonoBehaviour
        {
            public bool ExpandFirstInterludeSupportSelection;

            private EntityStateMachine stateMachine;

            private CombatDirector? combatDirector;

            public void Awake()
            {
                this.stateMachine = this.GetComponent<EntityStateMachine>();
                this.combatDirector = this.GetComponent<CombatDirector>();
                MonsterTracker.TrackCombatDirector(this.combatDirector);
            }

            public void OnEnable()
            {
                SpawnCard.onSpawnedServerGlobal += this.SpawnCard_onSpawnedServerGlobal;
            }

            public void OnDisable()
            {
                SpawnCard.onSpawnedServerGlobal -= this.SpawnCard_onSpawnedServerGlobal;
            }

            private void SpawnCard_onSpawnedServerGlobal(SpawnCard.SpawnResult result)
            {
                if (!result.success || !result.spawnedInstance || !result.spawnedInstance.TryGetComponent<MonsterTracker>(out var tracker) || tracker.combatDirector != this.combatDirector)
                {
                    return;
                }

                var body = Utils.GetCharacterBody(result.spawnedInstance);

                if (body)
                {
                    (this.stateMachine.state as SummonerStates.SummonerBaseState)?.OnBossSpawnedServer(result, body!);
                }
            }
        }

        public class SummonerUpgradeStrategy : UpgradeWaveStrategy
        {
            public override void UpgradeWave(InfiniteTowerWaveController wave)
            {
                var dir = wave.combatDirector;

                if (dir.TryGetComponent<SummonerBehavior>(out var behavior))
                {
                    behavior.ExpandFirstInterludeSupportSelection = true;
                }
            }
        }
    }
}