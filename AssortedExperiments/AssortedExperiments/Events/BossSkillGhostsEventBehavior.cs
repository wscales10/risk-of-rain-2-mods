using HG;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace AssortedExperiments.Events
{
    public delegate DirectorPlacementRule PlacementRuleGetter(CharacterBody ownerBody, SpawnCard spawnCard);

    public class BossSkillGhostsEventBehavior : MonoBehaviour
    {
        private readonly List<TimedGhostSummon> timedGhostSummons = new List<TimedGhostSummon>();

        public void Update() // FixedUpdate?
        {
            if (NetworkServer.active)
            {
                this.UpdateGhostSummons(Time.deltaTime);
            }
        }

        internal void OnBossSkillActivatedServer(GenericSkill skill)
        {
            var ownerBody = skill.characterBody;
            this.ProcessBossSkillUsage(new BossUseSkillContext
            {
                OwnerBody = ownerBody,
                SkillSlot = ownerBody.skillLocator.FindSkillSlot(skill)
            });
        }

        private static Action<SpawnCard.SpawnResult> OnCardSpawned(CharacterBody ownerBody) => (result) => OnCardSpawnedInternal(ownerBody, result);

        private static void OnCardSpawnedInternal(CharacterBody ownerBody, SpawnCard.SpawnResult result)
        {
            if (result.success)
            {
                CharacterMaster ownerMaster = ownerBody.master;
                if (ownerMaster && ownerMaster.minionOwnership.ownerMaster)
                {
                    ownerMaster = ownerMaster.minionOwnership.ownerMaster;
                }
                CharacterMaster spawnedMaster = result.spawnedInstance.GetComponent<CharacterMaster>();
                spawnedMaster.minionOwnership.SetOwner(ownerMaster);

                Inventory summonedInventory = spawnedMaster.GetComponent<Inventory>();
                if (summonedInventory)
                {
                    summonedInventory.CopyEquipmentFrom(ownerBody.inventory, false);
                    summonedInventory.GiveItemPermanent(Content.Items.EphemeralGhost);

                    // TODO: adjust health/damage
                    EliteDef eliteDefFromEquipmentIndex = EliteCatalog.GetEliteDefFromEquipmentIndex(ownerBody.inventory.currentEquipmentIndex);
                    if (eliteDefFromEquipmentIndex != null)
                    {
                        float num = eliteDefFromEquipmentIndex?.healthBoostCoefficient ?? 1f;
                        float num2 = eliteDefFromEquipmentIndex?.damageBoostCoefficient ?? 1f;
                        spawnedMaster.inventory.GiveItemPermanent(RoR2Content.Items.BoostHp, Mathf.RoundToInt((num - 1f) * 10f));
                        spawnedMaster.inventory.GiveItemPermanent(RoR2Content.Items.BoostDamage, Mathf.RoundToInt((num2 - 1f) * 10f));
                    }
                }
            }
        }

        private static void DisableSkill(CharacterBody body, Func<SkillLocator, GenericSkill> getSkill)
        {
            var skill = getSkill(body.skillLocator);

            if (skill)
            {
                skill.SetSkillOverride(body, CharacterBody.CommonAssets.disabledSkill, GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        private static void OnGhostBodyStart(CharacterBody body, SummonGhostContext ctx)
        {
            foreach (var banSkill in ctx.BanSkills)
            {
                DisableSkill(body, banSkill);
            }

            HealthComponent healthComponent = body.healthComponent;
            if (healthComponent)
            {
                healthComponent.Networkhealth = healthComponent.fullHealth * ctx.StartingHealthFraction;
            }

            if (NetworkServer.active)
            {
                body.master.EnsureComponent<MasterSuicideOnTimer>().lifeTimer = ctx.Lifespan; // TODO: ensure ghost deaths don't count toward sacrifice artifact etc
            }

            body.AddBuff(RoR2Content.Buffs.Intangible);
            Util.PlaySound("Play_item_proc_ghostOnKill", body.gameObject);
        }

        private void ProcessBossSkillUsage(BossUseSkillContext ctx)
        {
            var card = this.GetGhostCard(ctx);

            if (card is null)
            {
                Debug.LogWarning("Could not find suitable ghost summon card");
                return;
            }

            CharacterSpawnCard? ghostSpawnCard = card.SpawnCard;

            if (!ghostSpawnCard)
            {
                Debug.LogWarning($"Could not find spawn card '{card.SpawnCardName}'");
                return;
            }

            this.timedGhostSummons.Add(new TimedGhostSummon
            {
                context = new SummonGhostContext
                {
                    OwnerBody = ctx.OwnerBody,
                    GhostSpawnCard = ghostSpawnCard!,
                    GhostPlacementRule = card.GetPlacementRule(ctx.OwnerBody, ghostSpawnCard!),
                    Lifespan = card.Lifespan,
                    StartingHealthFraction = card.StartingHealthFraction,
                    BanSkills = card.BanSkills
                },
                timer = DelayGetter.GetDelay(ctx, card),
            });
        }

        private void UpdateGhostSummons(float deltaTime)
        {
            if (!NetworkServer.active)
            {
                this.ServerFunctionCalledOnClient();
                return;
            }

            for (int num = this.timedGhostSummons.Count - 1; num >= 0; num--)
            {
                var summon = this.timedGhostSummons[num];
                summon.timer -= deltaTime;

                if (summon.timer <= 0f)
                {
                    this.timedGhostSummons.RemoveAt(num);
                    this.TryToCreateGhost(summon.context);
                }
            }
        }

        private SummonGhostCard? GetGhostCard(BossUseSkillContext ctx)
        {
            IReadOnlyList<SummonGhostCard> options = GhostCardCatalog.Options;

            if (options.Count == 0)
            {
                return null;
            }

            var filters = new List<Func<SummonGhostCard, bool>>
            {
                card => card.IsAvailable
            };

            bool TryGetRandomWhere(Func<SummonGhostCard, bool> predicate, out SummonGhostCard? output)
            {
                var filtered = options.Where(predicate).ToArray();

                if (filtered.Length > 0)
                {
                    output = filtered.GetRandom();
                    return true;
                }

                output = null;
                return false;
            }

            if (ctx.OwnerBody.Is(RoR2Content.BodyPrefabs.VagrantBody) && ctx.SkillSlot == SkillSlot.Special) // TODO: add more logic like this?
            {
                filters.Add(card => card.SpawnCardName.Contains("Titan"));
            }

            if (!Utils.IsBossHealthBelowThreshold(BossGroup.FindBossGroup(ctx.OwnerBody), 0.4f))
            {
                filters.Add(card => !card.CanDoFriendlyFire);
            }

            filters.Add(card => card.IsChampion == ctx.OwnerBody.isChampion);

            if (TryGetRandomWhere(card => filters.All(x => x(card)), out var output))
            {
                return output;
            }

            return null;
        }

        private CharacterBody? TryToCreateGhost(SummonGhostContext ctx)
        {
            if (!NetworkServer.active)
            {
                this.ServerFunctionCalledOnClient();
                return null;
            }

            if (!ctx.GhostSpawnCard || !ctx.GhostSpawnCard.prefab)
            {
                return null;
            }

            // TODO: Try using a CombatDirector instead? I need to consider rate / amount limits too.
            var directorSpawnRequest = new DirectorSpawnRequest(ctx.GhostSpawnCard, ctx.GhostPlacementRule, RoR2Application.rng)
            {
                ignoreTeamMemberLimit = true,
                onSpawnedServer = OnCardSpawned(ctx.OwnerBody),
                summonerBodyObject = ctx.OwnerBody.gameObject,
                teamIndexOverride = ctx.OwnerBody.teamComponent.teamIndex
            };

            GameObject gameObject = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);

            if (!gameObject)
            {
                return null;
            }

            CharacterMaster summonedMaster = gameObject.GetComponent<CharacterMaster>();

            summonedMaster.onBodyStart += body => OnGhostBodyStart(body, ctx); // TODO: also remove this listener later, e.g. on master destroy?

            foreach (var ai in summonedMaster.AiComponents)
            {
                ai.fullVision = true;
                ai.xrayVision = true;
                ai.prioritizePlayers = true;
            }

            CharacterBody summonedBody = summonedMaster.GetBody();
            if (summonedBody) // This stuff seems to be necessary, but we can dig in further later
            {
                EntityStateMachine[] components = summonedBody.GetComponents<EntityStateMachine>();
                foreach (EntityStateMachine obj2 in components)
                {
                    obj2.initialStateType = obj2.mainStateType;
                }

                if (!summonedBody.bodyFlags.HasFlag(CharacterBody.BodyFlags.UsesAmbientLevel))
                {
                    Debug.LogWarning($"Summoned {Utils.GetBodyDisplayName(summonedBody)} does not use ambient level.");
                }
            }

            return summonedBody;
        }

        private sealed class TimedGhostSummon
        {
            public float timer;

            public SummonGhostContext context;
        }
    }
}