using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;

namespace BetterBossScaling
{
    [BepInPlugin("com.woodyscales.betterbossscaling", "Better Boss Scaling", "1.0.0")]
    [BepInDependency("com.rune580.riskofoptions")]
    public class BetterBossScaling : BaseUnityPlugin
    {
        private Config settings;

        private float? teleporterStartTime;

        public void Awake()
        {
            this.settings = new Config(this.Config);
            On.RoR2.SceneCatalog.OnActiveSceneChanged += this.SceneCatalog_OnActiveSceneChanged;
            On.RoR2.TeleporterInteraction.ChargingState.OnEnter += this.ChargingState_OnEnter;
            On.RoR2.BossGroup.OnMemberAddedServer += this.BossGroup_OnMemberAddedServer;
            On.RoR2.BossGroup.OnDefeatedServer += this.BossGroup_OnDefeatedServer;

            IL.RoR2.HealthComponent.Heal += this.HealthComponent_Heal;

            if (this.settings.DamageReducesBossMaxHealth.Value)
            {
                IL.RoR2.HealthComponent.TakeDamageProcess += this.HealthComponent_TakeDamageProcess;
            }
        }

        private static int? GetCurrentStageNumber()
        {
            int? stageClearCount = Run.instance?.stageClearCount;
            return stageClearCount is null ? null : stageClearCount + 1;
        }

        private static string str(object obj, string nullString = "??")
        {
            return obj is null ? nullString : obj.ToString();
        }

        private void HealthComponent_TakeDamageProcess(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(typeof(HealthComponent).FullName, nameof(HealthComponent.body)),
                x => x.MatchCallvirt(typeof(CharacterBody).FullName, "get_teamComponent"),
                x => x.MatchCallvirt(typeof(TeamComponent).FullName, "get_teamIndex"),
                x => x.MatchLdcI4((int)TeamIndex.Player),
                x => x.Match(OpCodes.Bne_Un_S));
            c.FindNext(
                out var cursors,
                x => x.MatchLdloc(11),
                x => x.MatchLdarg(0),
                x => x.MatchCall(typeof(HealthComponent).FullName, "get_fullCombinedHealth"),
                x => x.MatchDiv(),
                x => x.MatchLdcR4(100f),
                x => x.MatchMul());
            var applyCurse = cursors[0].MarkLabel();
            c.MoveAfterLabels();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<HealthComponent, bool>>(this.IsTeleporterBossHealthComponent);
            c.Emit(OpCodes.Brtrue_S, applyCurse);
        }

        private void HealthComponent_Heal(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(typeof(HealthComponent).FullName, nameof(HealthComponent.body)),
                x => x.MatchCallvirt(typeof(CharacterBody).FullName, "get_teamComponent"),
                x => x.MatchCallvirt(typeof(TeamComponent).FullName, "get_teamIndex"),
                x => x.MatchLdcI4((int)TeamIndex.Player),
                x => x.Match(OpCodes.Bne_Un_S));
            c.FindNext(
                out var cursors,
                x => x.MatchLdarg(1),
                x => x.Match(OpCodes.Ldc_R4),
                x => x.MatchDiv(),
                x => x.Match(OpCodes.Starg_S));
            var reduceHealing = cursors[0].MarkLabel();
            c.MoveAfterLabels();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<HealthComponent, bool>>(this.IsTeleporterBossHealthComponent);
            c.Emit(OpCodes.Brtrue_S, reduceHealing);
        }

        private bool IsTeleporterBossHealthComponent(HealthComponent hc)
        {
            var tp = TeleporterInteraction.instance;

            var teleporterBosses = tp?.bossGroup?.combatSquad?.readOnlyMembersList;
            var characterMaster = hc?.body?.master;

            if (teleporterBosses is null || characterMaster is null)
            {
                return false;
            }

            if (teleporterBosses.Contains(characterMaster))
            {
                this.Logger.LogDebug("Identified teleporter boss health component: " + str(characterMaster.name));
                return true;
            }
            else
            {
                if (characterMaster.isBoss)
                {
                    this.Logger.LogDebug("Non-teleporter boss health component: " + str(characterMaster.name));
                }

                return false;
            }
        }

        private void SceneCatalog_OnActiveSceneChanged(On.RoR2.SceneCatalog.orig_OnActiveSceneChanged orig, UnityEngine.SceneManagement.Scene oldScene, UnityEngine.SceneManagement.Scene newScene)
        {
            orig(oldScene, newScene);
            this.teleporterStartTime = null;
        }

        private void ChargingState_OnEnter(On.RoR2.TeleporterInteraction.ChargingState.orig_OnEnter orig, TeleporterInteraction.ChargingState self)
        {
            orig(self);
            this.teleporterStartTime = Run.instance.GetRunStopwatch();
        }

        private void BossGroup_OnDefeatedServer(On.RoR2.BossGroup.orig_OnDefeatedServer orig, BossGroup self)
        {
            var tp = TeleporterInteraction.instance;

            if (self == tp?.bossGroup)
            {
                TimeSpan? timeDiff = null;

                if (this.teleporterStartTime is float tpStartTime)
                {
                    float? currentTime = Run.instance?.GetRunStopwatch();

                    if (currentTime != null)
                    {
                        timeDiff = TimeSpan.FromSeconds(currentTime.Value - tpStartTime);
                    }
                }

                this.Logger.LogDebug($"Boss group defeated on stage {str(GetCurrentStageNumber())} with diff coef {str(Run.instance?.difficultyCoefficient)} when teleporter {str(tp.chargePercent)}% charged (took {str(timeDiff)}).");
            }

            orig(self);
        }

        private void BossGroup_OnMemberAddedServer(On.RoR2.BossGroup.orig_OnMemberAddedServer orig, BossGroup self, CharacterMaster memberMaster)
        {
            orig(self, memberMaster);

            if (self != TeleporterInteraction.instance?.bossGroup)
            {
                return;
            }

            var difficulty = Run.instance.selectedDifficulty;

            if (difficulty < DifficultyIndex.Normal)
            {
                return;
            }

            var hpDivisor = this.settings.HpDivisor.Value;

            if (difficulty > DifficultyIndex.Normal)
            {
                hpDivisor = (float)Math.Round(hpDivisor * 0.85f, 1);
            } 

            var damageDivisor = this.settings.DamageDivisor.Value;

            this.Logger.LogDebug($"Scaling boss '{memberMaster.name}'. Diff coef: {Run.instance.difficultyCoefficient}. HP divisor: {hpDivisor}. Damage divisor: {damageDivisor}.");
            memberMaster.ScaleDifficultyAsBoss(hpDivisor, damageDivisor, false);

            if (difficulty < DifficultyIndex.Hard || !this.settings.EnableAdaptiveArmor.Value)
            {
                return;
            }

            this.Logger.LogDebug("Giving boss adaptive armor: " + memberMaster.name);
            memberMaster.inventory.GiveItemPermanent(RoR2Content.Items.AdaptiveArmor);
        }
    }
}