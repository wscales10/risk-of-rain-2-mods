using AssortedExperiments.Items;
using EntityStates.Gup;
using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;

namespace AssortedExperiments.Events
{
    public class BossSkillGhostsEvent : Module
    {
        public override void Init()
        {
            On.RoR2.BossGroup.OnMemberAddedServer += this.BossGroup_OnMemberAddedServer;
            On.RoR2.BossGroup.OnMemberDefeatedServer += this.BossGroup_OnMemberDefeatedServer;
            On.RoR2.Run.Start += this.Run_Start;
            On.RoR2.TeleporterInteraction.Awake += this.TeleporterInteraction_Awake;
            IL.RoR2.CharacterModel.UpdateOverlays += CharacterModel_UpdateOverlays;
            IL.RoR2.CharacterModel.UpdateOverlayStates += CharacterModel_UpdateOverlayStates;
            IL.EntityStates.Gup.BaseSplitDeath.FixedUpdate += this.BaseSplitDeath_FixedUpdate;
        }

        private void BaseSplitDeath_FixedUpdate(ILContext il)
        {
            var c = new ILCursor(il);

            // Don't split Ephemeral Gups
            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<BaseSplitDeath>(nameof(BaseSplitDeath.characterSpawnCard)),
                x => x.MatchCall<UnityEngine.Object>("op_Implicit"),
                x => x.MatchBrfalse(out _));
            c.Index--;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<bool, BaseSplitDeath, bool>>((original, self) =>
            {
                var isEphemeralGhost = self.characterBody.inventory.Then2(x => x.GetItemCountEffective(Content.Items.EphemeralGhost) > 0) == true;
                return original && !isEphemeralGhost;
            });
        }

        private static void CharacterModel_UpdateOverlayStates(ILContext il)
        {
            var c = new ILCursor(il);
            c.GotoNext(MoveType.AfterLabel, x => x.MatchSet<CharacterModel>(nameof(CharacterModel.isGhost)));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<bool, CharacterModel, bool>>((original, self) => original || (self.body.inventory.Then2(x => x.GetItemCountEffective(Content.Items.EphemeralGhost) > 0) == true));
        }

        private static void CharacterModel_UpdateOverlays(ILContext il)
        {
            var c = new ILCursor(il);
            c.GotoNext(MoveType.AfterLabel, x => x.MatchSet<CharacterModel>(nameof(CharacterModel.isGhost)));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<bool, CharacterModel, bool>>((original, self) => original || (self.body.inventory.Then2(x => x.GetItemCountEffective(Content.Items.EphemeralGhost) > 0) == true));
        }

        private void TeleporterInteraction_Awake(On.RoR2.TeleporterInteraction.orig_Awake orig, TeleporterInteraction self)
        {
            orig(self);
            self.bossGroup.EnsureComponent<BossSkillGhostsEventBehavior>();
        }

        private void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
        {
            orig(self);
            GhostCardCatalog.Init();
        }

        private void BossGroup_OnMemberDefeatedServer(On.RoR2.BossGroup.orig_OnMemberDefeatedServer orig, BossGroup self, CharacterMaster memberMaster, DamageReport damageReport)
        {
            orig(self, memberMaster, damageReport);

            if (self.TryGetComponent<BossSkillGhostsEventBehavior>(out var behavior))
            {
                var body = memberMaster.GetBody();

                if (body)
                {
                    body.onSkillActivatedServer -= behavior.OnBossSkillActivatedServer;
                }
            }
        }

        private void BossGroup_OnMemberAddedServer(On.RoR2.BossGroup.orig_OnMemberAddedServer orig, RoR2.BossGroup self, RoR2.CharacterMaster memberMaster)
        {
            orig(self, memberMaster);

            if (self.TryGetComponent<BossSkillGhostsEventBehavior>(out var behavior))
            {
                memberMaster.GetBody().onSkillActivatedServer += behavior.OnBossSkillActivatedServer;
            }
        }
    }
}