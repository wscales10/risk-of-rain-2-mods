using BepInEx.Logging;
using EntityStates;
using HG;
using MonoMod.Cil;
using PactOfPunishment.Waves.Common;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace PactOfPunishment
{
    public static class Utils
    {
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

        public static IEnumerable<EliteDef> GetEliteDefs(SpawnCard spawnCard)
        {
            return CombatDirector.eliteTiers.Where(tier => tier.CanSelect(spawnCard.eliteRules)).SelectMany(tier => tier.eliteTypes.Where(elite => elite && elite.IsAvailable()));
        }

        public static IEnumerable<(BuffDef, Func<SpawnCard, bool>)> GetEliteBuffDefs()
        {
            return CombatDirector.eliteTiers.SelectMany(tier => tier.eliteTypes.Where(elite => elite && elite.IsAvailable()).Select(x =>
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

        public static void DisableSkill(CharacterBody body, Func<SkillLocator, GenericSkill> getSkill)
        {
            var skill = getSkill(body.skillLocator);

            if (skill)
            {
                skill.SetSkillOverride(body, CharacterBody.CommonAssets.disabledSkill, GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        public static CharacterBody? GetCharacterBody(GameObject entity)
        {
            if (entity.TryGetComponent<CharacterMaster>(out var master))
            {
                return master.GetBody();
            }

            return null;
        }

        public static void MakeUnscaledElite(Inventory inventory, EliteDef eliteDef)
        {
            inventory.SetEquipmentIndex(eliteDef.eliteEquipmentDef.equipmentIndex, false);
        }

        public static void MakeScaledElite(Inventory inventory, EliteDef? eliteDef)
        {
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

        public static IEnumerable<AISkillDriver> GetSkillDrivers(this CharacterMaster master, string customName)
        {
            return master.AiComponents.SelectMany(x => x.GetSkillDrivers(customName));
        }

        public static IEnumerable<AISkillDriver> GetSkillDrivers(this BaseAI ai, string customName)
        {
            return ai.skillDrivers.Where(x => x.customName == customName);
        }

        public static void OverrideCooldown(this CharacterBody body, Func<SkillLocator, GenericSkill> getSkill, float cooldown)
        {
            getSkill(body.skillLocator).overriddenRechargeInterval = cooldown;
            body.skillLocator.utility.RecalculateValues();
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

            var markAllDamageNonLethalBehavior = body!.healthComponent.EnsureComponent<SourceBasedDamageModifier>();
            markAllDamageNonLethalBehavior.sourceMask = DamageSource.SkillMask | DamageSource.Equipment;
            markAllDamageNonLethalBehavior.damageCoeficient = 0.75f;
            markAllDamageNonLethalBehavior.enabled = true;
            body.healthComponent.AddOnIncomingDamageServerReceiver(markAllDamageNonLethalBehavior);
        }

        public static void ForwardBossDamageTo(this HealthComponent bossHealthComponent, EntityStateMachine bossBodyStateMachine)
        {
            var receiver = bossHealthComponent.gameObject.AddComponent<OnBossTakeDamageServerReceiver>();
            receiver.stateMachine = bossBodyStateMachine;
            bossHealthComponent.AddOnTakeDamageServerReceiver(receiver);
        }

        public static AssetPromise<T> BeginLoad<T>(string key, ManualLogSource? logger = null)
        {
            var output = Addressables.LoadAssetAsync<T>(key);
            output.Completed += (result) =>
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

            return new AssetPromise<T>(output);
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

        public static void EliminateCombatSquadWhenLastMainMemberDies(this Component source, CombatSquad combatSquad, Func<CharacterMaster, bool> isMainMember, Action? callback = null)
        {
            DoSomethingWhenLastMainSquadMemberDies(combatSquad, isMainMember, (defeatedMember, damageReport) =>
            {
                try
                {
                    callback?.Invoke();
                }
                finally
                {
                    foreach (var member in combatSquad.readOnlyMembersList.Except(new[] { defeatedMember }).ToArray())
                    {
                        member.TrueKill(source.gameObject, source.gameObject, DamageType.VoidDeath);
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
        /// 
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
    }
}