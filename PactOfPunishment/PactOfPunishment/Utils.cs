using BepInEx.Logging;
using EntityStates;
using HG;
using MonoMod.Cil;
using PactOfPunishment.ProtectMonstersFromHazards;
using PactOfPunishment.Waves.Common;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using static RoR2.Navigation.NodeGraph;
using static RoR2.Navigation.NodeGraph.NodeFilters;

namespace PactOfPunishment
{
    public static partial class Utils
    {
        public static T InstantiateState<T>()
            where T : EntityState
        {
            return (T)EntityStateCatalog.InstantiateState(typeof(T));
        }

        public static IEnumerable<T> GetInvocationList<T>(T? e)
            where T : Delegate
        {
            if (e is null)
            {
                yield break;
            }

            foreach (var invocation in e.GetInvocationList().Cast<T>())
            {
                yield return invocation;
            }
        }

        public static IEnumerable<T> LogIfEmpty<T>(this IEnumerable<T> source)
        {
            bool isEmpty = true;

            foreach (var element in source)
            {
                isEmpty = false;
                yield return element;
            }

            if (isEmpty)
            {
                Debug.LogWarning($"Collection was empty{Environment.NewLine}{new System.Diagnostics.StackTrace()}");
            }
        }

        public static bool IsFoe(CharacterBody characterBody)
        {
            if (!characterBody)
            {
                return false;
            }

            var teamComponent = characterBody.teamComponent;
            return teamComponent && TeamManager.IsTeamEnemy(TeamIndex.Player, teamComponent.teamIndex);
        }

        public static BuffDef AddStatusEffect(Action<BuffDef> setup)
        {
            var buffDef = ScriptableObject.CreateInstance<BuffDef>();
            setup?.Invoke(buffDef);

            if (!ContentAddition.AddBuffDef(buffDef))
            {
                throw new InvalidOperationException();
            }

            return buffDef;
        }

        public static ILContext.Manipulator HookIL(this Action<ILCursor> hook)
        {
            return il =>
            {
                var c = new ILCursor(il);
                hook(c);
            };
        }

        public static WeightedSelection<EliteDef> GetEliteDefSelector(this CombatDirector combatDirector, SpawnCard spawnCard)
        {
            var selector = new WeightedSelection<EliteDef>();

            foreach (var choice in EliteTiers.Instance.GetEliteTiers(combatDirector).Where(tier => tier.CanSelect(spawnCard.eliteRules)).SelectMany(tier => tier.eliteTypes.Where(elite => elite && elite.IsAvailable()).Select(elite => (elite, tier.costMultiplier))))
            {
                selector.AddChoice(choice.elite, 1 / Mathf.Max(0.5f, choice.costMultiplier));
            }

            return selector;
        }

        public static IEnumerable<EliteDef> GetEliteDefs(this CombatDirector combatDirector, SpawnCard spawnCard)
        {
            return EliteTiers.Instance.GetEliteTiers(combatDirector).Where(tier => tier.CanSelect(spawnCard.eliteRules)).SelectMany(tier => tier.eliteTypes.Where(elite => elite && elite.IsAvailable()));
        }

        public static IEnumerable<EliteDef> GetEliteDefsFromCheapestAvailableTier(this CombatDirector combatDirector, SpawnCard spawnCard)
        {
            return EliteTiers.Instance.GetEliteTiers(combatDirector).Where(tier => tier.CanSelect(spawnCard.eliteRules)).Select(tier => tier.eliteTypes.Where(elite => elite && elite.IsAvailable())).FirstOrDefault(x => x.Any()) ?? Enumerable.Empty<EliteDef>();
        }

        public static IEnumerable<(BuffDef, Func<SpawnCard, bool>)> GetEliteBuffDefs(this CombatDirector combatDirector)
        {
            return EliteTiers.Instance.GetEliteTiers(combatDirector).SelectMany(tier => tier.eliteTypes.Where(elite => elite && elite.IsAvailable()).Select(x =>
            {
                return (x.eliteEquipmentDef.passiveBuffDef, (Func<SpawnCard, bool>)(card => tier.CanSelect(card.eliteRules)));
            }));
        }

        public static void RemoveWhere<T>(this WeightedSelection<T> weightedSelection, Func<T, bool> predicate)
        {
            for (int i = weightedSelection.Count - 1 - 1; i >= 0; i--)
            {
                var choice = weightedSelection.GetChoice(i);

                if (predicate(choice.value))
                {
                    weightedSelection.RemoveChoice(i);
                }
            }
        }

        public static void AddChoicesWithRelativeWeight<T>(this WeightedSelection<T> weightedSelection, float weightOfOriginalSelection, Func<T, bool> predicate, params (T value, float weight)[] choices)
        {
            float totalWeight = weightedSelection.getTotalWeight();
            float weightMultiplier;

            if (weightOfOriginalSelection <= 0)
            {
                weightedSelection.Clear();
                weightMultiplier = 1;
            }
            else if (Mathf.Approximately(totalWeight, 0))
            {
                weightMultiplier = 1;
            }
            else
            {
                weightMultiplier = totalWeight / weightOfOriginalSelection;
            }

            foreach (var (value, weight) in choices.Where(x => predicate?.Invoke(x.value) != false))
            {
                weightedSelection.AddChoice(value, weight * weightMultiplier);
            }
        }

        public static void DisableSkill(this object source, CharacterBody body, SkillSlot skillSlot)
        {
            var skill = body.skillLocator.GetSkill(skillSlot);

            if (skill)
            {
                skill.SetSkillOverride(source, CharacterBody.CommonAssets.disabledSkill, GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        public static void EnableSkill(this object source, CharacterBody body, SkillSlot skillSlot)
        {
            var skill = body.skillLocator.GetSkill(skillSlot);

            if (skill)
            {
                skill.UnsetSkillOverride(source, CharacterBody.CommonAssets.disabledSkill, GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        public static bool TryGetCharacterBody(GameObject entity, out CharacterBody body)
        {
            if (entity && entity.TryGetComponent<CharacterMaster>(out var master))
            {
                body = master.GetBody();
                return true;
            }

            body = default;
            return false;
        }

        public static void MakeUnscaledEliteUsingEquipment(this CharacterBody body, EliteDef eliteDef)
        {
            MakeUnscaledEliteUsingEquipment(body.inventory ??= body.master.inventory, eliteDef.eliteEquipmentDef);
        }

        public static void MakeUnscaledEliteUsingBuff(this CharacterBody body, EliteDef eliteDef)
        {
            MakeUnscaledEliteUsingBuff(body, eliteDef.eliteEquipmentDef.passiveBuffDef);
        }

        public static void MakeUnscaledEliteUsingEquipment(Inventory inventory, EquipmentDef eliteEquipmentDef)
        {
            inventory.SetEquipmentIndex(eliteEquipmentDef.equipmentIndex, false);
        }

        public static void MakeUnscaledEliteUsingBuff(this CharacterBody body, BuffDef eliteBuffDef)
        {
            body.AddBuff(eliteBuffDef);
        }

        public static void MakeScaledElite(this CharacterBody body, EliteDef? eliteDef)
        {
            var inventory = body.inventory ??= body.master.inventory;

            EquipmentIndex equipmentIndex = eliteDef?.eliteEquipmentDef?.equipmentIndex ?? EquipmentIndex.None;
            if (equipmentIndex != EquipmentIndex.None)
            {
                inventory.SetEquipmentIndex(equipmentIndex, false);
            }

            float num = eliteDef?.healthBoostCoefficient ?? 1f;
            float num2 = eliteDef?.damageBoostCoefficient ?? 1f;
            inventory.GiveItemPermanent(RoR2Content.Items.BoostHp, Mathf.RoundToInt((num - 1f) * 10f));
            inventory.GiveItemPermanent(RoR2Content.Items.BoostDamage, Mathf.RoundToInt((num2 - 1f) * 10f));
        }

        public static IEnumerable<AISkillDriver> GetSkillDrivers(this CharacterMaster master, SkillSlot skillSlot)
        {
            return master.GetSkillDriversInternal(x => x.skillSlot == skillSlot).LogIfEmpty();
        }

        public static IEnumerable<AISkillDriver> GetSkillDrivers(this BaseAI ai, SkillSlot skillSlot)
        {
            return ai.GetSkillDriversInternal(x => x.skillSlot == skillSlot).LogIfEmpty();
        }

        public static IEnumerable<AISkillDriver> GetSkillDrivers(this CharacterMaster master, string customName)
        {
            return master.GetSkillDriversInternal(x => x.customName == customName).LogIfEmpty();
        }

        public static IEnumerable<AISkillDriver> GetSkillDrivers(this BaseAI ai, string customName)
        {
            return ai.GetSkillDriversInternal(x => x.customName == customName).LogIfEmpty();
        }

        public static void OnLoad<TObject>(string key, Action<TObject> onLoad)
        {
            Addressables.LoadAssetAsync<TObject>(key).Completed += x => onLoad(x.Result);
        }

        public static void AddMinion(this CharacterBody body, GameObject minion, DeployableSlot deployableSlot)
        {
            var deployable = minion.AddComponent<Deployable>();
            deployable.onUndeploy = new UnityEvent();
            deployable.onUndeploy.AddListener(new UnityAction(minion.GetComponent<CharacterMaster>().TrueKill));
            body.master.AddDeployable(deployable, deployableSlot);
        }

        public static void ScaleMaxHealth(this CharacterBody body, object source, float multiplier)
        {
            body.EnsureComponent<MultiplyMaxHealthBehavior>().Multipliers[source] = multiplier;
        }

        public static void ScaleDamage(this CharacterBody body, object source, float multiplier)
        {
            body.EnsureComponent<MultiplyDamageBehavior>().Multipliers[source] = multiplier;
        }

        public static void MakeBodySemiImmortal(CharacterBody? body)
        {
            if (!body)
            {
                return;
            }

            var markAllDamageNonLethalBehavior = body!.healthComponent.EnsureComponent<MakeAllDamageNonLethalBehavior>();
            markAllDamageNonLethalBehavior.enabled = true;
            body.healthComponent.AddOnIncomingDamageServerReceiver(markAllDamageNonLethalBehavior);
        }

        public static void MakeBodyMortal(CharacterBody? body)
        {
            if (body && body!.TryGetComponent<MakeAllDamageNonLethalBehavior>(out var behavior))
            {
                behavior.enabled = false;
            }
        }

        public static void ResistNonTargetedDamage(this CharacterBody? body)
        {
            if (!body)
            {
                return;
            }

            var damageModifier = body!.healthComponent.EnsureComponent<SourceBasedDamageModifier>();
            damageModifier.sourceMask = DamageSource.SkillMask | DamageSource.Equipment;
            damageModifier.damageCoeficient = 0.5f;
            damageModifier.enabled = true;
            body.healthComponent.AddOnIncomingDamageServerReceiver(damageModifier);
        }

        public static void ForwardBossDamageTo(this HealthComponent bossHealthComponent, EntityStateMachine bossBodyStateMachine)
        {
            var receiver = bossHealthComponent.gameObject.AddComponent<OnBossTakeDamageServerReceiver>();
            receiver.stateMachine = bossBodyStateMachine;
            bossHealthComponent.AddOnTakeDamageServerReceiver(receiver);
        }

        public static AssetPromise<T> BeginLoad<T>(string key, ManualLogSource? logger = null)
        {
            var asyncOperationHandle = Addressables.LoadAssetAsync<T>(key);
            asyncOperationHandle.Completed += (result) =>
            {
                if (result.OperationException is Exception ex)
                {
                    if (logger is null)
                    {
                        Debug.LogException(ex);
                    }
                    else
                    {
                        logger.LogError(ex);
                    }
                }
            };

            return new AssetPromise<T>(asyncOperationHandle);
        }

        public static AssetPromise<U> BeginLoadAndTransform<T, U>(string key, Func<T, U> transform, ManualLogSource? logger = null)
        {
            var asyncOperationHandle = Addressables.LoadAssetAsync<T>(key);
            asyncOperationHandle.Completed += (result) =>
            {
                if (result.OperationException is Exception ex)
                {
                    if (logger is null)
                    {
                        Debug.LogException(ex);
                    }
                    else
                    {
                        logger.LogError(ex);
                    }
                }
            };

            return new AssetPromise<U>(Addressables.ResourceManager.CreateChainOperation(asyncOperationHandle, x =>
            {
                var status = x.Status;

                return status switch
                {
                    AsyncOperationStatus.Succeeded => Addressables.ResourceManager.CreateCompletedOperation(transform(x.Result), null),
                    AsyncOperationStatus.Failed => Addressables.ResourceManager.CreateCompletedOperationWithException(default(U), x.OperationException),
                    _ => Addressables.ResourceManager.CreateCompletedOperation(default(U), $"Unexpected operation status '{status}'"),
                };
            }));
        }

        public static SerializableEntityStateType AddEntityState<T>(ManualLogSource? logger = null)
            where T : EntityState
        {
            var output = ContentAddition.AddEntityState<T>(out bool wasAdded);

            if (!wasAdded)
            {
                LogError(logger, "Failed to add entity state.");
            }

            return output;
        }

        public static void LogError(ManualLogSource? logger, object errorData)
        {
            if (logger is null)
            {
                Debug.LogError(errorData);
            }
            else
            {
                logger.LogError(errorData);
            }
        }

        public static void DisableStunsEtc(this CharacterBody bossBody)
        {
            var component = bossBody.GetComponent<SetStateOnHurt>();

            if (component)
            {
                component.canBeFrozen = false;
                component.canBeStunned = false;
                component.canBeHitStunned = false;
            }
        }

        public static void DirectHeal(HealthComponent healthComponent, float healthFraction)
        {
            if (!healthComponent)
            {
                Debug.LogWarning($"Trying to heal nonexistent health component to {Mathf.RoundToInt(healthFraction * 100)}%");
                return;
            }

            float targetCombinedHealth = healthComponent.fullCombinedHealth * Mathf.Clamp01(healthFraction);
            float currentCombinedHealth = healthComponent.health + healthComponent.shield;
            float combinedHealthToAdd = targetCombinedHealth - currentCombinedHealth;

            if (combinedHealthToAdd <= 0)
            {
                return;
            }

            float healthToAdd = Mathf.Min(combinedHealthToAdd, healthComponent.fullHealth - healthComponent.health);

            if (healthToAdd > 0)
            {
                combinedHealthToAdd -= healthToAdd;
                healthComponent.Networkhealth += healthToAdd;
            }

            float shieldToAdd = Mathf.Min(combinedHealthToAdd, healthComponent.fullShield - healthComponent.shield);

            if (shieldToAdd > 0)
            {
                healthComponent.Networkshield += shieldToAdd;
            }
        }

        public static bool ScaleDeathRewards(CharacterBody? body, float multiplier)
        {
            if (body && body!.TryGetComponent<DeathRewards>(out var deathRewards))
            {
                ScaleDeathRewards(deathRewards, multiplier);
                return true;
            }

            return false;
        }

        public static void ScaleDeathRewards(DeathRewards deathRewards, float multiplier)
        {
            deathRewards.spawnValue = (int)Mathf.Max(1f, deathRewards.spawnValue * multiplier);
            deathRewards.expReward = (uint)Mathf.Ceil(deathRewards.expReward * multiplier);
            deathRewards.goldReward = (uint)Mathf.Ceil(deathRewards.goldReward * multiplier);
        }

        public static void EliminateCombatSquadWhenLastMainMemberDies(this GameObject source, CombatSquad combatSquad, Func<CharacterMaster, bool> isMainMember, Func<CharacterMaster, bool>? shouldBeEliminated = null, Action? callback = null)
        {
            shouldBeEliminated ??= _ => true;

            DoSomethingWhenLastMainSquadMemberDies(combatSquad, isMainMember, (defeatedMember, damageReport) =>
            {
                try
                {
                    callback?.Invoke();
                }
                finally
                {
                    foreach (var member in combatSquad.readOnlyMembersList.Except(new[] { defeatedMember }).Where(shouldBeEliminated).ToArray())
                    {
                        member.TrueKill(source, source, DamageType.VoidDeath);
                    }
                }
            });
        }

        public static void DoSomethingWhenLastMainSquadMemberDies(CombatSquad combatSquad, Func<CharacterMaster, bool> isMainMember, Action<CharacterMaster, DamageReport>? callback)
        {
            combatSquad.onMemberDefeatedServer += CombatSquad_onMemberDefeatedServer;

            void CombatSquad_onMemberDefeatedServer(CharacterMaster defeatedMember, DamageReport damageReport)
            {
                if (!combatSquad.readOnlyMembersList.Any(x => x != defeatedMember && isMainMember(x)))
                {
                    callback?.Invoke(defeatedMember, damageReport);
                }
            }
        }

        public static void AddSpawnListener(this CombatDirector combatDirector, UnityAction<GameObject> listener)
        {
            (combatDirector.onSpawnedServer ??= new CombatDirector.OnSpawnedServer()).AddListener(listener);
        }

        public static void GotoLast(this ILCursor c, params Func<Mono.Cecil.Cil.Instruction, bool>[] predicates)
        {
            c.Index = c.Instrs.Count - 1;
            c.GotoPrev(predicates);
        }

        public static void GotoLast(this ILCursor c, MoveType moveType, params Func<Mono.Cecil.Cil.Instruction, bool>[] predicates)
        {
            c.Index = c.Instrs.Count - 1;
            c.GotoPrev(moveType, predicates);
        }

        public static float GetAltitude(this CharacterBody body)
        {
            if (Physics.Raycast(body.footPosition, Vector3.down, out var hitInfo, 1000, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
            {
                return hitInfo.distance;
            }

            return float.PositiveInfinity;
        }

        public static bool Is(this CharacterBody? body, CharacterBody bodyPrefab)
        {
            return body?.bodyIndex == bodyPrefab.bodyIndex;
        }

        public static bool IsOneOf(this CharacterBody? body, params CharacterBody[] bodyPrefabs)
        {
            return bodyPrefabs.Any(body.Is);
        }

        /// <summary>
        /// </summary>
        /// <param name="body"></param>
        /// <param name="hpDivisor"></param>
        /// <param name="damageDivisor"></param>
        /// <param name="wasSpawnedByCombatDirector"></param>
        /// <param name="wasCoefFactoredIntoSpawning">"true" case not implemented yet</param>
        /// <exception cref="NotImplementedException"></exception>
        public static void ScaleDifficultyAsBoss(this CharacterBody body, float hpDivisor, float damageDivisor, bool wasSpawnedByCombatDirector, bool wasCoefFactoredIntoSpawning)
        {
            if (wasCoefFactoredIntoSpawning)
            {
                throw new NotImplementedException();
            }
            else
            {
                body.master.ScaleDifficultyAsBoss(hpDivisor, damageDivisor, !wasSpawnedByCombatDirector);
            }
        }

        public static EntityState? GetSafeWardState()
        {
            var run = Run.instance as InfiniteTowerRun;

            if (!run)
            {
                return null;
            }

            var safeWardController = run!.safeWardController;

            if (safeWardController && safeWardController.wardStateMachine)
            {
                return safeWardController.wardStateMachine.state;
            }

            return null;
        }

        public static void DebugDrawNodeGraph(NodeGraph nodeGraph, HullClassification hullClassification)
        {
            nodeGraph.DebugDrawLinks(hullClassification);

            foreach (var node in nodeGraph.nodes)
            {
                Gizmos.DrawSphere(node.position, 0.5f);
            }
        }

        public static void OverrideRechargeStock(this GenericSkill skill, RecalculateStats.GetRechargeStockDelegate getRechargeStock)
        {
            RecalculateStats.OverrideRechargeStock(skill, getRechargeStock);
        }

        public static void EnsureSafeMinimumInterruptPriority(GenericSkill skill)
        {
            EntityStateMachine? stateMachine = skill.stateMachine;

            if (!stateMachine)
            {
                Debug.LogWarning($"Unable to find state machine for '{skill}' skill on '{skill.characterBody}'.");
            }

            RecalculateStats.SetMinimumInterruptPriorityOverride(stateMachine, skill.activationState.stateType, state =>
            {
                var orig = state.GetMinimumInterruptPriority();
                var safeMinimum = skill.interruptPriority + 1;
                return safeMinimum > orig ? safeMinimum : orig;
            });
        }

        public static DirectorCardCategorySelection MakeDirectorCardCategorySelection(params (string categoryName, SpawnCards spawnCards)[] categories)
        {
            var dccs = ScriptableObject.CreateInstance<DirectorCardCategorySelection>();

            for (int i = 0; i < categories.Length; i++)
            {
                var (categoryName, spawnCards) = categories[i];
                dccs.AddCategory(categoryName, 1);
                foreach (var spawnCard in spawnCards.GetSpawnCards())
                {
                    dccs.AddCard(i, new DirectorCard
                    {
                        selectionWeight = 1,
                        spawnCard = spawnCard,
                    });
                }
            }

            return dccs;
        }

        public static Vector3 GetHorizontalFacingDirection(this CharacterBody body)
        {
            var aimDirection = body.inputBank.aimDirection;
            return new Vector3(aimDirection.x, 0, aimDirection.z).normalized;
        }

        public static bool IsSafeLocation(Vector3 position)
        {
            if (Run.instance is InfiniteTowerRun run && run.fogDamageController.enabled) // TODO: apply to other fog damage controllers too? to lava?
            {
                return run.fogDamageController.safeZones.Any(x => x.IsInBounds(position));
            }

            return true;
        }

        public static void InsertSkillDriver(this BaseAI ai, AISkillDriver newSkillDriver, int index)
        {
            var skillDrivers = ai.skillDrivers;
            ArrayUtils.ArrayInsert(ref skillDrivers, index, newSkillDriver);
            ai.ReplaceSkillDrivers(skillDrivers);
        }

        public static void RemoveSkillDriversWhere(this BaseAI ai, Func<AISkillDriver, bool> predicate)
        {
            var skillDrivers = ai.skillDrivers;

            for (int i = skillDrivers.Length - 1; i >= 0; i--)
            {
                var skillDriver = skillDrivers[i];

                if (predicate(skillDriver))
                {
                    ArrayUtils.ArrayRemoveAtAndResize(ref skillDrivers, i);
                    UnityEngine.Object.Destroy(skillDriver);
                }
            }

            ai.ReplaceSkillDrivers(skillDrivers);
        }

        [Server]
        public static void SetStunBypassImmunity(this SetStateOnHurt self, float duration)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning($"[Server] function 'System.Void {typeof(Utils).FullName}::SetStunBypassImmunity({typeof(SetStateOnHurt).FullName}, System.Single)' called on client");
                return;
            }

            if (self.hasEffectiveAuthority)
            {
                self.SetStunInternal(duration);
                return;
            }

            self.CallRpcSetStun(duration);
        }

        public static void DoDuringGameLoad(Action action)
        {
            if (RoR2Application.loadFinished)
            {
                action();
            }
            else
            {
                RoR2Application.onLoad = (Action)Delegate.Combine(RoR2Application.onLoad, action);
            }
        }

        public static NodeIndex FindClosestSafeNode(this NodeGraph self, Vector3 position, HullClassification hullClassification, float maxDistance = float.PositiveInfinity)
        {
            var nodeSearchFilter = Create(self, And(new NodeHullFilter(hullClassification), default(NodeAvailableFilter), default(NodeSafeFilter)));

            if (self.blockMap.GetNearestItemWhichPassesFilter(position, maxDistance, ref nodeSearchFilter, out NodeIndex result))
            {
                return result;
            }

            return NodeIndex.invalid;
        }

        public static void InterceptLoadField<TSelf, TField>(this ILCursor c, string fieldName, Func<TSelf, TField> func)
        {
            while (c.TryGotoNext(MoveType.AfterLabel, x => x.MatchLdfld<TSelf>(fieldName)))
            {
                c.Remove();
                c.MoveAfterLabels(); // AfterLabel stuff is probably not needed here, but just to be safe...
                c.EmitDelegate(func);
            }
        }

        public static void SetupCombatDirectorPrefabForAddsSpawning(this CombatDirector dir, DirectorCardCategorySelection monsterCards, uint maxSquadCount, float spawnFrequency, float spawnFrequencyVariation, float expectedDifficultyCoefficient)
        {
            dir._monsterCards = monsterCards;
            dir.maxSquadCount = maxSquadCount;
            dir.minRerollSpawnInterval = 0.5f;
            dir.maxRerollSpawnInterval = 0.5f;
            dir.minSeriesSpawnInterval = spawnFrequency - spawnFrequencyVariation;
            dir.maxSeriesSpawnInterval = spawnFrequency + spawnFrequencyVariation;
            dir.moneyWaveIntervals = new RangeFloat[]
            {
                new RangeFloat
                {
                    min = 1,
                    max = 1
                }
            };
            dir.creditMultiplier = monsterCards.categories.SelectMany(x => x.cards).Max(x => x.cost) / (1 + .4f * expectedDifficultyCoefficient) / spawnFrequency;
            dir.EnsureComponent<DisableWhileSquadFullBehavior>();
        }

        public static void EnsureHasItem(this CharacterBody body, ItemDef itemDef)
        {
            int count = (body.inventory ??= body.master.inventory).GetItemCountPermanent(itemDef);

            if (count < 1)
            {
                body.inventory.GiveItemPermanent(itemDef, 1 - count);
            }
        }

        private static IEnumerable<AISkillDriver> GetSkillDriversInternal(this CharacterMaster master, Func<AISkillDriver, bool> predicate)
        {
            return master?.AiComponents?.SelectMany(x => x.GetSkillDriversInternal(predicate)) ?? Enumerable.Empty<AISkillDriver>();
        }

        private static IEnumerable<AISkillDriver> GetSkillDriversInternal(this BaseAI ai, Func<AISkillDriver, bool> predicate)
        {
            return ai.skillDrivers.Where(predicate);
        }
    }
}