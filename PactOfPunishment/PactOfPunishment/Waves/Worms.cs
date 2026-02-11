using EntityStates;
using HG;
using PactOfPunishment.Conditions;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PactOfPunishment.Waves
{
    public partial class Worms : MainBossWaveDefinition<InfiniteTowerBossWaveController>
    {
        protected override UpgradeWaveStrategy GetUpgradeStrategy()
        {
            return new NullUpgradeStrategy(); // TODO: extreme measures
        }

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerBossWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);
            SetupStateMachinePrefab(wavePrefab.gameObject.AddComponent<EntityStateMachine>());

            dir.maxSquadCount = 1;
            wavePrefab.immediateCreditsFraction = 0.45f;
            wavePrefab.guaranteeInitialChampion = true;

            // TODO: boss wave start UI is not accurate? same for all bosses?
            wavePrefab.gameObject.AddComponent<WormsBehavior>();
        }

        private static void SetupStateMachinePrefab(EntityStateMachine x)
        {
            x.customName = nameof(Worms);
            x.initialStateType = new SerializableEntityStateType(typeof(WormStates.PreFight));

            // x.mainStateType = new SerializableEntityStateType(typeof(WormStates.PreFight));
            x.commonComponents = new EntityStateMachine.CommonComponentCache(x.gameObject);

            // x.nextStateModifier = this.ModifyNextState;
        }

        public static class WormStates
        {
            public struct References
            {
                public CharacterBody? MainBossBody;

                public int MainBossMonsterIndex;
            }

            public abstract class WormsBaseState : EntityState
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

                    if (nextState is WormsBaseState worms)
                    {
                        worms.References = this.References;
                    }
                }

                protected static void ApplyHealthMultiplier(CharacterBody body, float healthMultiplier) // TODO: also scale damage?
                {
                    // We could use more combinations of boost and cut to get it more accurate, but
                    // I don't think it's worth it.
                    var requiredCutStacks = Mathf.CeilToInt(1 / healthMultiplier - 1);
                    body!.inventory.GiveItemPermanent(RoR2Content.Items.CutHp, requiredCutStacks);

                    var requiredBoostStacks = Mathf.FloorToInt(10 * (healthMultiplier * (requiredCutStacks + 1) - 1));
                    body!.inventory.GiveItemPermanent(RoR2Content.Items.BoostHp, requiredBoostStacks);
                }
            }

            public class PreFight : WormsBaseState
            {
                public override void OnEnter()
                {
                    Debug.Log("Entering PreFight");
                    base.OnEnter();
                    var combatDirector = this.outer.GetComponent<CombatDirector>();
                    var selector = combatDirector.finalMonsterCardsSelection;
                    this.References.MainBossMonsterIndex = selector.EvaluateToChoiceIndex(combatDirector.rng.nextNormalizedFloat);
                    combatDirector.OverrideNextBossCard(selector.GetChoice(this.References.MainBossMonsterIndex).value, false);
                }

                public override void OnBossSpawnedServer(SpawnCard.SpawnResult result, CharacterBody body)
                {
                    base.OnBossSpawnedServer(result, body);
                    float healthMultiplier = 800f / result.spawnRequest.spawnCard.directorCreditCost; // TODO: is this correct?
                    Debug.Log($"Scaling health for {body.name} by {healthMultiplier}");
                    ApplyHealthMultiplier(body, healthMultiplier);
                    this.SetupBossHealthComponent(body.healthComponent);
                    this.References.MainBossBody = body; // TODO: what about swarms?
                    this.outer.SetState(new Phase1());
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

                protected override WormsBaseState GetNextState() => new FirstInterlude();
            }

            public class Phase2 : PhaseState // TODO: upgrade boss in later phases
            {
                public override float PhaseEndHealthThreshold => 1f / 3;

                protected override WormsBaseState GetNextState() => new SecondInterlude();
            }

            public class Phase3 : PhaseState
            {
                public override float PhaseEndHealthThreshold => 0;

                public override void OnEnter()
                {
                    base.OnEnter();
                    MakeBodyMortal(this.References.MainBossBody);
                }

                protected override WormsBaseState GetNextState() => new Death();

                private static void MakeBodyMortal(CharacterBody? body)
                {
                    if (!body)
                    {
                        return;
                    }

                    body!.healthComponent.gameObject.GetComponent<MakeAllDamageNonLethalBehavior>().enabled = false;
                }
            }

            public class Death : WormsBaseState
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

                protected override WormsBaseState GetNextState() => new Phase2();
            }

            public class SecondInterlude : InterludeState
            {
                public override float PhaseStartingHealthFraction => 1f / 3;

                public override void OnEnter()
                {
                    this.SupportToSpawn = 6;
                    base.OnEnter();
                }

                protected override WormsBaseState GetNextState() => new Phase3();
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
                    float healthMultiplier = 120f / result.spawnRequest.spawnCard.directorCreditCost; // TODO: is this correct?
                    float myMaxHealth = body.healthComponent.fullCombinedHealth;
                    float? bossMaxHealth = this.References.MainBossBody?.healthComponent.fullCombinedHealth;

                    if (myMaxHealth > 0 && bossMaxHealth != null)
                    {
                        healthMultiplier = Mathf.Min(healthMultiplier, bossMaxHealth.Value * 0.15f / myMaxHealth);
                    }

                    Debug.Log($"Scaling health for {body.name} by {healthMultiplier}");
                    ApplyHealthMultiplier(body, healthMultiplier);
                    this.combatSquad!.AddMember(body.master);
                }

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

                    var combatDirector = this.outer.GetComponent<CombatDirector>();
                    var wave = this.outer.GetComponent<InfiniteTowerWaveController>();
                    var selector = combatDirector.finalMonsterCardsSelection;
                    DirectorCard directorCard = selector.GetChoice(selector.EvaluateToChoiceIndex(combatDirector.rng.nextNormalizedFloat, new int[] { this.References.MainBossMonsterIndex })).value;

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

            public abstract class MainBossAliveState : WormsBaseState
            {
                protected abstract WormsBaseState GetNextState();
            }
        }

        public sealed class OnTakeDamageServerReceiver : MonoBehaviour, IOnTakeDamageServerReceiver
        {
            public EntityStateMachine stateMachine;

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (this.stateMachine.state is WormStates.WormsBaseState wormsState)
                {
                    wormsState.OnMainBossDamageTaken(damageReport.victim);
                }
            }
        }

        public class WormsBehavior : MonoBehaviour
        {
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
                    (this.stateMachine.state as WormStates.WormsBaseState)?.OnBossSpawnedServer(result, body!);
                }
            }
        }
    }
}