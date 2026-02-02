using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.UI;


namespace AssortedExperiments.Items
{
    public class AssortedItems : Module
    {
        // private static readonly Lazy<AssetBundle> assetBundle;

        private readonly Dictionary<ScoreboardController, Action<TeamComponent, TeamIndex>> rebuildDelegates = new Dictionary<ScoreboardController, Action<TeamComponent, TeamIndex>>();

        private ExplicitPickupDropTable summonGolemsDropTable;

        private delegate void Modifier<in TSelf>(TSelf self, ref float num);

        private delegate float HealthRegenModifier(float num, HealthComponent self);

        public override void Init()
        {
            //SetAssetBundle()
            RegisterContent();
            var bossDropTable = ScriptableObject.CreateInstance<ExplicitPickupDropTable>();
            bossDropTable.name = "dtBossHalcyonite";
            bossDropTable.canDropBeReplaced = true;
            bossDropTable.pickupEntries = new ExplicitPickupDropTable.PickupDefEntry[] { new ExplicitPickupDropTable.PickupDefEntry { pickupDef = Content.Items.SummonGolems, pickupWeight = 1 } };
            this.summonGolemsDropTable = bossDropTable;

            On.RoR2.HoldoutZoneController.Start += HoldoutZoneController_Start;
            IL.RoR2.HoldoutZoneController.DoUpdate += HoldoutZoneController_DoUpdate;
            On.RoR2.ItemCatalog.Init += ItemCatalog_Init;

            IL.RoR2.BloodSiphonNearbyController.SearchForTargets += BloodSiphonNearbyController_SearchForTargets;
            On.RoR2.BloodSiphonNearbyController.Tick += BloodSiphonNearbyController_Tick;

            On.RoR2.GameModeCatalog.SetGameModes += this.GameModeCatalog_SetGameModes;
            IL.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;

            //IL.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;

            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            IL.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;

            IL.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += CharacterBody_AddTimedBuff_BuffDef_float;

            IL.RoR2.UI.ScoreboardController.Rebuild += ScoreboardController_Rebuild;
            On.RoR2.UI.ScoreboardController.OnEnable += this.ScoreboardController_OnEnable;
            On.RoR2.UI.ScoreboardController.OnDisable += this.ScoreboardController_OnDisable;

            On.RoR2.UI.ItemInventoryDisplay.ItemIsVisible += ItemInventoryDisplay_ItemIsVisible;
            /* Not using this at the moment
            On.RoR2.Inventory.CalculateEquipmentCooldownScale += Inventory_CalculateEquipmentCooldownScale;*/
            On.RoR2.GenericSkill.CalculateFinalRechargeInterval += GenericSkill_CalculateFinalRechargeInterval;

            IL.RoR2.HealthComponent.Heal += HealthComponent_Heal;
            IL.RoR2.HealthComponent.ServerFixedUpdate += HealthComponent_ServerFixedUpdate;
            On.RoR2.HealthComponent.Awake += HealthComponent_Awake;

            On.RoR2.CombatSquad.AddMember += this.CombatSquad_AddMember;
            On.RoR2.BossGroup.OnMemberDefeatedServer += this.BossGroup_OnMemberDefeatedServer;

            IL.RoR2.HealthComponent.UpdateLastHitTime += this.HealthComponent_UpdateLastHitTime;
            On.RoR2.CharacterMaster.OnServerStageBegin += this.CharacterMaster_OnServerStageBegin;
        }

        private static void HealthComponent_Awake(On.RoR2.HealthComponent.orig_Awake orig, HealthComponent self)
        {
            orig(self);
            self.gameObject.AddComponent<RegenTickController>();
        }

        private static float GenericSkill_CalculateFinalRechargeInterval(On.RoR2.GenericSkill.orig_CalculateFinalRechargeInterval orig, GenericSkill self)
        {
            // TODO: use IL instead
            float num = self.cooldownOverride > 0f ? self.cooldownOverride : self.baseRechargeInterval;
            return Mathf.Max(Mathf.Min(num, 0.5f), num * self.cooldownScale - self.flatCooldownReduction) + self.temporaryCooldownPenalty;
        }

        private static Action<TeamComponent, TeamIndex> RebuildScoreboard(ScoreboardController self)
        {
            return (_, teamIndex) =>
            {
                if (teamIndex == TeamIndex.Player)
                {
                    self.Rebuild();
                }
            };
        }

        private static void ScoreboardController_Rebuild(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(x => x.MatchStloc(0));
            c.Emit(OpCodes.Ldarg_0);

            c.EmitDelegate<Action<List<PlayerCharacterMasterController>, ScoreboardController>>((list, self) =>
            {
                var masterList = list.Select(x => x.master).SelectMany(x => new CharacterMaster[] { x }.Concat(MinionInheritOnKillBodyBehavior.GetMinions(x))).ToList();
                self.SetStripCount(masterList.Count);

                for (int i = 0; i < masterList.Count; i++)
                {
                    self.stripAllocator.elements[i].SetMaster(masterList[i]);
                }

                EnableScrolling(self);
            });

            c.RemoveRange(c.Instrs.Count - c.Index - 1);
        }

        private static void EnableScrolling(ScoreboardController self)
        {
            var container = self.transform.GetChild(0).gameObject;
            var stripContainer = container.transform.GetChild(1).gameObject;
            var stripScroll = AddScroller(container, stripContainer);
            self.StartCoroutine(UpdateInventoriesHeightNextFrame((RectTransform)stripScroll.transform));
        }

        private static GameObject AddScroller(GameObject container, GameObject stripContainer)
        {
            if (stripContainer.name == "StripScroll")
            {
                return stripContainer;
            }

            // Record properties of StripContainer transform
            RectTransform transform = (RectTransform)stripContainer.transform;
            var anchorMin = transform.anchorMin;
            var anchorMax = transform.anchorMax;
            var offsetMin = transform.offsetMin;
            var offsetMax = transform.offsetMax;
            var position = transform.position;
            var pivot = transform.pivot;
            var sizeDelta = transform.sizeDelta;

            // Create StripScroll
            var stripScroll = new GameObject("StripScroll", typeof(RectTransform), typeof(ScrollRect));
            var scrollTransform = (RectTransform)stripScroll.transform;
            scrollTransform.SetParent(container.transform, false);

            // Match original StripContainer properties
            scrollTransform.SetSiblingIndex(stripContainer.transform.GetSiblingIndex());
            scrollTransform.anchorMin = anchorMin;
            scrollTransform.anchorMax = anchorMax;
            scrollTransform.offsetMin = offsetMin;
            scrollTransform.offsetMax = offsetMax;
            scrollTransform.position = position;
            scrollTransform.pivot = pivot;
            scrollTransform.sizeDelta = sizeDelta;

            // Create Viewport
            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            var viewportTransform = (RectTransform)viewport.transform;
            viewportTransform.SetParent(stripScroll.transform, false);
            viewportTransform.anchorMin = Vector2.zero;
            viewportTransform.anchorMax = Vector2.one;
            viewportTransform.offsetMin = Vector2.zero;
            viewportTransform.offsetMax = Vector2.zero;

            // Reparent StripContainer
            transform.SetParent(viewport.transform, false);
            transform.anchorMin = new Vector2(0, 1);
            transform.anchorMax = new Vector2(1, 1);
            transform.pivot = new Vector2(0.5f, 1);
            transform.anchoredPosition = Vector2.zero;
            transform.offsetMin = Vector2.zero;
            transform.offsetMax = Vector2.zero;

            // Configure ScrollRect
            var scrollRect = stripScroll.GetComponent<ScrollRect>();
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = transform;
            scrollRect.vertical = true;
            scrollRect.horizontal = false;
            scrollRect.scrollSensitivity = 40;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            stripScroll.AddComponent<AdaptablePreferredHeight>().ContentToGrowWith = transform;

            return stripScroll;
        }

        private static IEnumerator UpdateInventoriesHeightNextFrame(RectTransform scrollTransform)
        {
            yield return null;
            LayoutRebuilder.MarkLayoutForRebuild(scrollTransform);
        }

        private static void RegisterContent()
        {
            Content.Items.SoulShard = AddItem(ItemTier.Lunar, "SoulShard");
            Content.Items.BloodShard = AddItem(ItemTier.Lunar, "BloodShard");
            Content.Items.StackingSlowOnHit = AddItem(ItemTier.Tier1, "StackingSlowOnHit");
            Content.Items.MinionInheritOnKill = AddItem(ItemTier.Tier2, "MinionInheritOnKill"); // TODO: move "minion inventories on inventory screen" to other mod?
            Content.Items.Heavy = AddItem(ItemTier.Lunar, "Heavy");
            Content.Items.Stim = AddItem(ItemTier.Lunar, "Stim");
            Content.Items.InstantDeathMark = AddItem(ItemTier.NoTier, "InstantDeathMark", item =>
            {
                item.hidden = true;
                item.canRemove = false;
            });
            Content.Items.SummonGolems = AddItem(ItemTier.Boss, "SummonGolems");
            Content.Items.SummonGolemsConsumed = AddItem(ItemTier.NoTier, "SummonGolemsConsumed");
            Content.Items.EphemeralGhost = AddItem(ItemTier.NoTier, "EphemeralGhost", item => item.canRemove = false);
            Content.Buffs.Slow1Stacking = AddStatusEffect(buffDef =>
            {
                buffDef.name = "Slow1Buff";
                buffDef.canStack = true;
                buffDef.isDebuff = true;
                buffDef.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Junk/Common/texBuffSlow25Icon.tif").WaitForCompletion();
            });

            Content.Buffs.Stim = AddStatusEffect(buffDef =>
            {
                buffDef.name = "Stim";
                buffDef.canStack = true;
                buffDef.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Junk/Common/texBuffBodyArmorIcon.tif").WaitForCompletion();
                buffDef.stackingDisplayMethod = BuffDef.StackingDisplayMethod.Percentage;
            });

            var summonGolemsDirectorPrefab = PrefabAPI.CreateEmptyPrefab(nameof(SummonGolemsBodyBehavior));
            var combatDirector = summonGolemsDirectorPrefab.AddComponent<CombatDirector>();
            SummonGolemsBodyBehavior.SetupCombatDirectorPrefab(combatDirector);
            SummonGolemsBodyBehavior.CombatDirectorPrefab = summonGolemsDirectorPrefab;
        }

        private static void BloodSiphonNearbyController_SearchForTargets(ILContext il)
        {
            var c = new ILCursor(il);
            c.GotoNext(x => x.MatchCallvirt<SphereSearch>(nameof(SphereSearch.OrderCandidatesByDistance)));
            c.EmitDelegate<Func<SphereSearch, SphereSearch>>((search) =>
            {
                BloodShardController.Filter(ref search.searchData);
                return search;
            });

            c.GotoNext(MoveType.After, x => x.MatchCallvirt<SphereSearch>(nameof(SphereSearch.GetHurtBoxes)));
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Action<List<HurtBox>>>(BloodShardController.Sort);
        }

        private static void HoldoutZoneController_Start(On.RoR2.HoldoutZoneController.orig_Start orig, HoldoutZoneController self)
        {
            orig(self);

            if (self.applyFocusConvergence)
            {
                self.gameObject.AddComponent<LunarShardsController>();
            }
        }

        private static ItemDef AddItem(ItemTier tier, string name, Action<ItemDef>? setup = null)
        {
            var customItem = new CustomItem(
                name,
                $"ITEM_{name.ToUpperInvariant()}_NAME",
                $"ITEM_{name.ToUpperInvariant()}_DESC",
                $"ITEM_{name.ToUpperInvariant()}_LORE",
                $"ITEM_{name.ToUpperInvariant()}_PICKUP",
                null,
                null,
                Array.Empty<ItemTag>(),
                tier,
                false,
                true,
                null,
                null);
            customItem.ItemDef.deprecatedTier = tier; // Have to set this for untiered items, and seems sensible to set it for all. I guess I could fix the bug that causes it to be required...
            setup?.Invoke(customItem.ItemDef!);

            if (!ItemAPI.Add(customItem))
            {
                throw new InvalidOperationException();
            }

            return customItem.ItemDef!;
        }

        /*private static async Task<Sprite> LoadSpriteAsync(string name)
        {
            string keyPath = "";
            throw new NotImplementedException();

            return await Addressables.LoadAssetAsync<Sprite>("RoR2/Junk/Common/texBuffSlow25Icon.tif").Task;
        }*/

        private static BuffDef AddStatusEffect(Action<BuffDef> setup)
        {
            var buffDef = ScriptableObject.CreateInstance<BuffDef>();
            setup?.Invoke(buffDef);

            if (!ContentAddition.AddBuffDef(buffDef))
            {
                throw new InvalidOperationException();
            }

            return buffDef;
        }

        private static void HoldoutZoneController_DoUpdate(ILContext il)
        {
            var c = new ILCursor(il);
            c.GotoNext(x => x.MatchLdfld<HoldoutZoneController>(nameof(HoldoutZoneController.chargeRadiusDelta)));
            c.Remove();
            c.EmitDelegate<Func<HoldoutZoneController, float>>((holdoutZone) =>
            {
                var currentSoulShardCount = holdoutZone.TryGetComponent<SoulShardController>(out var soulShardController) ? soulShardController.CurrentItemCount : 0;

                if (currentSoulShardCount > 0)
                {
                    // TODO: improve this - it would feel unfair if there was ever an effect which is supposed to grow the radius rather than shrink it.
                    return Mathf.Min(holdoutZone.chargeRadiusDelta, -0.6f * holdoutZone.baseRadius);
                }
                else
                {
                    return holdoutZone.chargeRadiusDelta;
                }
            });

            c.GotoNext(x => x.MatchLdfld<HoldoutZoneController>(nameof(HoldoutZoneController.dischargeRate)));
            c.Remove();
            c.EmitDelegate<Func<HoldoutZoneController, float>>((holdoutZone) =>
            {
                var currentSoulShardCount = holdoutZone.TryGetComponent<SoulShardController>(out var soulShardController) ? soulShardController.CurrentItemCount : 0;
                return holdoutZone.dischargeRate + currentSoulShardCount / 30f;
            });

            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(0),
                x => x.MatchCall<HoldoutZoneController>($"get_{nameof(HoldoutZoneController.charge)}"),
                x => x.MatchLdloc(6),
                x => x.MatchLdarg(1),
                x => x.MatchMul(),
                x => x.MatchAdd(),
                x => x.MatchCall<Mathf>(nameof(Mathf.Clamp01)),
                x => x.MatchCall<HoldoutZoneController>($"set_{nameof(HoldoutZoneController.charge)}"));
            c.GotoNext(MoveType.After, x => x.MatchLdloc(6));
            c.Emit(OpCodes.Dup);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<float, HoldoutZoneController>>((holdoutZoneChargePerSecond, holdoutZone) =>
            {
                if (holdoutZone.TryGetComponent<LunarShardsController>(out var lunarShardsController))
                {
                    lunarShardsController.HoldoutZoneChargeRateMultiplier = holdoutZoneChargePerSecond * holdoutZone.baseChargeDuration;
                }
            });
        }

        private static void ItemCatalog_Init(On.RoR2.ItemCatalog.orig_Init orig)
        {
            orig();
            Content.Items.SoulShard.tags = ArrayUtils.Clone(RoR2Content.Items.FocusConvergence.tags);
            Content.Items.BloodShard.tags = ArrayUtils.Clone(RoR2Content.Items.FocusConvergence.tags);
            AddTags(Content.Items.StackingSlowOnHit, ItemTag.Utility, ItemTag.CanBeTemporary);
            AddTags(Content.Items.MinionInheritOnKill, ItemTag.Utility, ItemTag.CannotCopy); // TODO: Can be temporary?
            AddTags(Content.Items.Heavy, ItemTag.Damage, ItemTag.Utility);
            AddTags(Content.Items.Stim, ItemTag.Utility, ItemTag.Damage, ItemTag.MobilityRelated);
            AddTags(Content.Items.InstantDeathMark, ItemTag.Utility, ItemTag.Damage); // TODO: are both of these needed?
            AddTags(Content.Items.SummonGolems, ItemTag.Utility, ItemTag.Damage, ItemTag.CannotCopy, ItemTag.AIBlacklist, ItemTag.LowHealth);
            AddTags(Content.Items.SummonGolemsConsumed, ItemTag.Utility, ItemTag.Damage, ItemTag.OnStageBeginEffect);
        }

        private static void AddTags(ItemDef itemDef, params ItemTag[] tags)
        {
            itemDef.tags = ArrayUtils.Join(itemDef.tags, tags.Except(itemDef.tags).ToArray());
        }

        private static void BloodSiphonNearbyController_Tick(On.RoR2.BloodSiphonNearbyController.orig_Tick orig, BloodSiphonNearbyController self)
        {
            orig(self);

            var holdoutZone = self.holdoutZone;

            if (!holdoutZone)
            {
                return;
            }

            if (!holdoutZone.TryGetComponent<BloodShardController>(out var bloodShardController))
            {
                return;
            }

            bloodShardController.UpdateHealthFractionCoefficients();
        }

        private static void HealthComponent_TakeDamageProcess(ILContext il)
        {
            var c = new ILCursor(il);
            int damageVariable = 0;

            // Bonus damage lunar item
            c.GotoNext(MoveType.AfterLabel,
                x => x.MatchLdloc(1),
                x => x.MatchCallvirt<CharacterMaster>($"get_{nameof(CharacterMaster.inventory)}"),
                x => x.MatchLdsfld(typeof(DLC1Content.Items), nameof(DLC1Content.Items.FragileDamageBonus)),
                x => x.MatchCallvirt<Inventory>(nameof(Inventory.GetItemCountEffective)),
                x => x.MatchStloc(out _),
                x => x.MatchLdloc(out _),
                x => x.MatchLdcI4(0),
                x => x.MatchBle(out _),
                x => x.MatchLdloc(out _),
                x => x.MatchLdcR4(1),
                x => x.MatchLdloc(out _),
                x => x.MatchConvR4(),
                x => x.MatchLdcR4(out _),
                x => x.MatchMul(),
                x => x.MatchAdd(),
                x => x.MatchMul(),
                x => x.MatchStloc(out damageVariable));
            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ldloca_S, (byte)damageVariable);
            c.EmitDelegate<Modifier<DamageInfo>>((DamageInfo damageInfo, ref float num) =>
            {
                var damageSource = damageInfo.damageType.damageSource;

                if (!damageSource.HasFlag(DamageSource.Primary) && !damageSource.HasFlag(DamageSource.Secondary))
                {
                    return;
                }

                var inventory = damageInfo.attacker.Then(x => x.GetComponent<CharacterBody>()).Then(x => x.inventory);

                if (!inventory)
                {
                    return;
                }

                int itemCount = inventory!.GetItemCountEffective(Content.Items.Heavy);

                if (itemCount > 0)
                {
                    num *= 1f + 0.5f * itemCount;
                }
            });

            // On-hit debuffs
            c.GotoNext(MoveType.AfterLabel,
                x => x.MatchLdloc(0),
                x => x.MatchLdfld(out _),
                x => x.MatchLdfld<DamageInfo>(nameof(DamageInfo.damageType)),
                x => x.MatchLdcI4(8),
                x => x.MatchCall<DamageTypeCombo>("op_Implicit"),
                x => x.MatchCall<DamageTypeCombo>("op_BitwiseAnd"),
                x => x.MatchCall<DamageTypeCombo>("op_Implicit"),
                x => x.MatchBrfalse(out _));
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Action<HealthComponent, DamageInfo>>((self, damageInfo) =>
            {
                var inventory = damageInfo.attacker.Then(x => x.GetComponent<CharacterBody>()).Then(x => x.inventory);

                if (!inventory)
                {
                    return;
                }

                var instantDeathMarkCount = inventory!.GetItemCountEffective(Content.Items.InstantDeathMark);

                if (instantDeathMarkCount > 0 && damageInfo.procCoefficient > 0)
                {
                    self.body.AddTimedBuff(RoR2Content.Buffs.DeathMark, instantDeathMarkCount * damageInfo.procCoefficient);
                }

                var itemCount = inventory!.GetItemCountEffective(Content.Items.StackingSlowOnHit);

                for (int i = 0; i < itemCount; i++)
                {
                    self.body.AddTimedBuff(Content.Buffs.Slow1Stacking, 0.4f, 999);
                }
            });
        }

        private static void CharacterBody_AddTimedBuff_BuffDef_float(ILContext il)
        {
            var c = new ILCursor(il);
            c.Index = c.Instrs.Count - 1;
            c.GotoPrev(MoveType.AfterLabel,
                x => x.MatchLdarg(0),
                x => x.MatchLdloca(0),
                x => x.MatchCall(out var methodRef) && methodRef.DeclaringType.Name == nameof(CharacterBody) && methodRef.Name.Contains("DefaultBehavior"));
            c.FindPrev(
                out var cursors,
                x => x.MatchLdloc(0),
                x => x.MatchLdfld(out _),
                x => x.MatchLdsfld(typeof(DLC3Content.Buffs), nameof(DLC3Content.Buffs.CritChanceAndDamage)),
                x => x.MatchCall<UnityEngine.Object>("op_Equality"),
                x => x.MatchBrfalse(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdloca(0),
                x => x.MatchCall(out _),
                x => x.MatchPop(),
                x => x.MatchLdarg(0),
                x => x.MatchLdloca(0),
                x => x.MatchCall(out _),
                x => x.MatchRet());
            var defaultLabel = c.DefineLabel();
            for (int i = 0; i < cursors.Length; i++)
            {
                if (i == 2)
                {
                    c.Emit(OpCodes.Ldsfld, typeof(Content.Buffs).GetFieldCached(nameof(Content.Buffs.Slow1Stacking)));
                }
                else if (i == 4)
                {
                    c.Emit(OpCodes.Brfalse_S, defaultLabel);
                }
                else
                {
                    c.Emit(cursors[i].Next.OpCode, cursors[i].Next.Operand);
                }
            }
            c.MarkLabel(defaultLabel);
        }

        private static bool ItemInventoryDisplay_ItemIsVisible(On.RoR2.UI.ItemInventoryDisplay.orig_ItemIsVisible orig, ItemIndex itemIndex)
        {
            // TODO: use IL hooking instead
            ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
            return itemDef != null && !itemDef.hidden && itemDef.nameToken != null && Language.GetString(itemDef.nameToken) != itemDef.nameToken;
        }

        private static float StimProcessHeal(float amount, HealthComponent self)
        {
            var component = self.body.Then(x => x.GetComponent<StimBodyBehavior>());

            if (component)
            {
                component!.ProcessHeal(ref amount, self.fullCombinedHealth);
            }

            return amount;
        }

        private static void HealthComponent_Heal(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(MoveType.AfterLabel,
                x => x.MatchLdarg(1),
                x => x.MatchStloc(2),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.health)),
                x => x.MatchLdarg(0),
                x => x.MatchCall<HealthComponent>($"get_{nameof(HealthComponent.fullHealth)}"),
                x => x.MatchBgeUn(out _));
            c.Emit(OpCodes.Ldarg_1); // Could use Ldarga but this works fine
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<HealthRegenModifier>(StimProcessHeal);
            c.Emit(OpCodes.Starg_S, (byte)1);
        }

        private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            // Effect of stacking slow debuff
            int stackingSlowBuffCount = sender.GetBuffCount(Content.Buffs.Slow1Stacking);
            args.moveSpeedReductionMultAdd += stackingSlowBuffCount * 0.01f;

            var inventory = sender.inventory;
            if (inventory)
            {
                // Heavy item
                var heavyItemCount = inventory.GetItemCountEffective(Content.Items.Heavy);
                if (heavyItemCount > 0)
                {
                    // TODO: consider whether it should transform some bonus attack speed into bonus damage, or affect band proccing
                    args.attackSpeedTotalMult /= 1f + 0.5f * heavyItemCount;
                    args.secondarySkill.cooldownMultiplier *= 1f + 0.5f * heavyItemCount;
                }

                var buffCount = sender.GetBuffCount(Content.Buffs.Stim);

                if (buffCount > 0)
                {
                    var stimAmount = buffCount / 100f;
                    args.moveSpeedMultAdd += stimAmount * 1.25f;
                    args.damageMultAdd += stimAmount * 8;
                    args.armorAdd += stimAmount * 200;
                }

                if (inventory.GetItemCountEffective(Content.Items.InstantDeathMark) > 0)
                {
                    args.levelMoveSpeedAdd += 0.05f; // TODO: should probably create a separate item for this effect, but I'm just testing it at the moment so I'll wait until I know whether I like it
                }
            }
        }

        private void CharacterMaster_OnServerStageBegin(On.RoR2.CharacterMaster.orig_OnServerStageBegin orig, CharacterMaster self, Stage stage)
        {
            orig(self, stage);

            if (NetworkServer.active)
            {
                new Inventory.ItemTransformation
                {
                    originalItemIndex = Content.Items.SummonGolemsConsumed.itemIndex,
                    newItemIndex = Content.Items.SummonGolems.itemIndex,
                    maxToTransform = int.MaxValue,
                    transformationType = ItemTransformationTypeIndex.None, // Should this be something?
                }.TryTransform(self.inventory, out _);
            }
        }

        private void HealthComponent_UpdateLastHitTime(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(MoveType.AfterLabel,
                x => x.MatchLdarg(0),
                x => x.MatchLdflda<HealthComponent>(nameof(HealthComponent.itemCounts)),
                x => x.MatchLdfld<HealthComponent.ItemCounts>(nameof(HealthComponent.ItemCounts.fragileDamageBonus)),
                x => x.MatchLdcI4(0),
                x => x.MatchBle(out _));

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<HealthComponent>>(self =>
            {
                try
                {
                    var inventory = self.body.inventory;
                    
                    if (!inventory)
                    {
                        return;
                    }

                    if (self.isHealthLow && new Inventory.ItemTransformation
                    {
                        originalItemIndex = Content.Items.SummonGolems.itemIndex,
                        newItemIndex = Content.Items.SummonGolemsConsumed.itemIndex,
                        maxToTransform = int.MaxValue,
                        allowWhenDisabled = true // really?
                    }.TryTransform(self.body.inventory, out _)) // TODO: set value of local variable tryTransformResult?
                    {
                        var director = self.body.GetComponent<CombatDirectorAnchor>().CombatDirector;

                        if (director)
                        {
                            SummonGolemsBodyBehavior.EliminateSquad(director!.combatSquad);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex);
                }
            });
        }

        private void BossGroup_OnMemberDefeatedServer(On.RoR2.BossGroup.orig_OnMemberDefeatedServer orig, BossGroup self, CharacterMaster memberMaster, DamageReport damageReport)
        {
            orig(self, memberMaster, damageReport);

            if (!self.bossDropTablesLocked && self.bossDropTables.Count == 0)
            {
                self.bossDropTables.Add(this.summonGolemsDropTable);
            }
        }

        private void CombatSquad_AddMember(On.RoR2.CombatSquad.orig_AddMember orig, CombatSquad self, CharacterMaster memberMaster)
        {
            orig(self, memberMaster);

            if (self.GetComponent<HalcyoniteShrineInteractable>() && memberMaster.name == "HalcyoniteMaster(Clone)")
            {
                memberMaster.EnsureComponent<DeathRewards>().bossDropTable = this.summonGolemsDropTable;
            }
        }

        private static void CharacterBody_RecalculateStats(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchCall<CharacterBody>($"get_{nameof(CharacterBody.healthComponent)}"),
                x => x.MatchLdloc(out _),
                x => x.MatchLdloca(out _),
                x => x.MatchInitobj<ProcChainMask>(),
                x => x.MatchLdloc(out _),
                x => x.MatchLdcI4(0),
                x => x.MatchCallvirt<HealthComponent>(nameof(HealthComponent.Heal)),
                x => x.MatchPop(),
                x => x.MatchBr(out _));

            c.GotoNext(x => x.MatchCallvirt<HealthComponent>(nameof(HealthComponent.Heal)));
            c.Remove();
            c.EmitDelegate<On.RoR2.HealthComponent.orig_Heal>((self, amount, _procChainMask, _nonRegen) =>
            {
                if (!NetworkServer.active || !self.alive)
                {
                    return 0f;
                }

                self.recentlyTookDamageCoyoteTimer = 0.2f;
                float num = self.health;

                if (self.health < self.fullHealth)
                {
                    float num4 = Mathf.Max(Mathf.Min(amount, self.fullHealth - self.health), 0f);
                    self.Networkhealth = self.health + num4;
                }

                if (self.health > num && self.health >= self.fullHealth)
                {
                    self.body.MarkAllStatsDirty();
                }

                return self.health - num;
            });
        }

        private static void HealthComponent_ServerFixedUpdate(ILContext il)
        {
            var c = new ILCursor(il);

            // Modify regen accumulation code
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Action<HealthComponent, float>>((self, deltaTime) => self.GetComponent<RegenTickController>().TickDown(deltaTime));

            c.GotoNext(MoveType.AfterLabel,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.regenAccumulator)),
                x => x.MatchLdcR4(1),
                x => x.MatchBleUn(out _));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<HealthComponent>>(self =>
            {
                var regenTicker = self.GetComponent<RegenTickController>();

                if (regenTicker.Timer <= 0)
                {
                    regenTicker.Reset();
                    float num = self.regenAccumulator;

                    if (num > 0)
                    {
                        self.regenAccumulator = 0;
                        self.Heal(num, default, false);
                    }
                }
            });

            // Process shield regen
            c.GotoNext(
                x => x.MatchLdloc(4),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.body)),
                x => x.MatchCallvirt<CharacterBody>($"get_{nameof(CharacterBody.maxShield)}"),
                x => x.MatchLdcR4(0.5f),
                x => x.MatchMul(),
                x => x.MatchLdarg(1),
                x => x.MatchMul(),
                x => x.MatchAdd(),
                x => x.MatchStloc(4));
            c.GotoNext(x => x.MatchAdd());
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<HealthRegenModifier>(StimProcessHeal);
        }

        //private static void CharacterBody_RecalculateStats(ILContext il)
        //{
        //    // Effect of stacking slow debuff
        //    var c = new ILCursor(il);
        //    c.GotoNext(MoveType.AfterLabel,
        //        x => x.MatchLdarg(0),
        //        x => x.MatchLdsfld(typeof(DLC3Content.Buffs), nameof(DLC3Content.Buffs.Slow10Stacking)),
        //        x => x.MatchCall<CharacterBody>(nameof(CharacterBody.GetBuffCount)),
        //        x => x.MatchStloc(out _),
        //        x => x.MatchLdloc(out _),
        //        x => x.MatchBrfalse(out _));
        //    c.FindNext(out var cursors,
        //        x => x.MatchLdloc(out _),
        //        x => x.MatchLdloc(out _),
        //        x => x.MatchConvR4(),
        //        x => x.MatchLdcR4(0.1f),
        //        x => x.MatchMul(),
        //        x => x.MatchAdd(),
        //        x => x.MatchStloc(out _));
        //    var variableNumber = cursors[1].Next.Operand;
        //    c.Emit(OpCodes.Ldarg_0);
        //    c.Emit(OpCodes.Ldloca_S, variableNumber);
        //    static void ApplyStackingSlowEffect(CharacterBody self, ref float num)
        //    {
        //        int buffCount = self.GetBuffCount(Content.Buffs.Slow1Stacking);
        //        num += buffCount * 0.01f;
        //    }
        //    c.EmitDelegate<Modifier<CharacterBody>>(ApplyStackingSlowEffect);

        // // Heavy item secondary cooldown increase int cooldownVariable = 0; int
        // secondaryCooldownVariable = 0; c.GotoNext(MoveType.AfterLabel, x => x.MatchLdarg(0), x =>
        // x.MatchCall<CharacterBody>($"get_{nameof(CharacterBody.teamComponent)}"), x =>
        // x.MatchCallvirt<TeamComponent>($"get_{nameof(TeamComponent.teamIndex)}"), x =>
        // x.MatchLdcI4(2), x => x.MatchBneUn(out _), x =>
        // x.MatchCall<Run>($"get_{nameof(Run.instance)}"), x =>
        // x.MatchCallvirt<Run>($"get_{nameof(Run.selectedDifficulty)}"), x => x.MatchLdcI4(9), x =>
        // x.MatchBlt(out _), x => x.MatchLdloc(out cooldownVariable)); c.FindNext(out _, x =>
        // x.MatchCallvirt<SkillLocator>($"get_{nameof(SkillLocator.secondaryBonusStockSkill)}"), x
        // => x.MatchLdloc(cooldownVariable), x => x.MatchLdloc(out secondaryCooldownVariable));

        // c.Emit(OpCodes.Ldarg_0); c.Emit(OpCodes.Ldloca_S, (byte)secondaryCooldownVariable);
        // c.EmitDelegate<Modifier<CharacterBody>>((CharacterBody self, ref float num) => { var
        // inventory = self.inventory;

        // if (!inventory) { return; }

        // var itemCount = inventory.GetItemCountEffective(Content.Items.Heavy);

        // if (itemCount <= 0) { return; }

        //        num *= 1f + 0.5f * itemCount;
        //    });
        //}
        private void ScoreboardController_OnDisable(On.RoR2.UI.ScoreboardController.orig_OnDisable orig, ScoreboardController self)
        {
            if (this.rebuildDelegates.TryGetValue(self, out var rebuildDelegate))
            {
                TeamComponent.onJoinTeamGlobal -= rebuildDelegate;
                TeamComponent.onLeaveTeamGlobal -= rebuildDelegate;
                this.rebuildDelegates.Remove(self);
            }

            orig(self);
        }

        private void ScoreboardController_OnEnable(On.RoR2.UI.ScoreboardController.orig_OnEnable orig, ScoreboardController self)
        {
            orig(self);

            var rebuildDelegate = RebuildScoreboard(self);
            this.rebuildDelegates[self] = rebuildDelegate;
            TeamComponent.onJoinTeamGlobal += rebuildDelegate;
            TeamComponent.onLeaveTeamGlobal += rebuildDelegate;
        }

        private void GameModeCatalog_SetGameModes(On.RoR2.GameModeCatalog.orig_SetGameModes orig, Run[] newGameModePrefabComponents)
        {
            orig(newGameModePrefabComponents);

            if (!(GameModeCatalog.FindGameModePrefabComponent("InfiniteTowerRun") is InfiniteTowerRun simulacrumPrefab))
            {
                this.Logger.LogWarning("Could not find InfiniteTowerRun prefab.");
                return;
            }

            simulacrumPrefab.blacklistedItems = ArrayUtils.Join(simulacrumPrefab.blacklistedItems, new ItemDef[]
            {
                Content.Items.SoulShard,
                Content.Items.BloodShard
            });
        }
    }
}