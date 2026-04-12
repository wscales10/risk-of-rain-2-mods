using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Projectile;
using System;

namespace PactOfPunishment.Conditions
{
    public sealed class IncreaseEnvironmentalDamage : DefaultConditionDef
    {
        public override int MaxRank => 1;

        private const float damageMultiplier = 5f;

        public override string Description => string.Format(base.Description, Utils.Percent(damageMultiplier - 1));

        public static DamageAPI.ModdedDamageType DamageType => Content.DamageTypes.EnvironmentalHazard;

        public delegate DamageTypeCombo DamageTypeComboModifier(DamageTypeCombo damageType);

        public static DamageTypeCombo AddDamageType(DamageTypeCombo damageType)
        {
            damageType.AddModdedDamageType(DamageType);
            return damageType;
        }

        public override void Init()
        {
            Content.DamageTypes.EnvironmentalHazard = DamageAPI.ReserveDamageType();
            On.RoR2.HealthComponent.TakeDamageProcess += this.HealthComponent_TakeDamageProcess;

            IL.EntityStates.Destructible.ExplosivePotDeath.Explode += Utils.HookIL(ExplosivePotDeath_Explode);
            IL.EntityStates.Destructible.FusionCellDeath.Explode += Utils.HookIL(ExplosivePotDeath_Explode);
            IL.EntityStates.Destructible.LunarRainDeathState.Explode += Utils.HookIL(ExplosivePotDeath_Explode);
            IL.EntityStates.Destructible.SulfurPodDeath.Explode += Utils.HookIL(ExplosivePotDeath_Explode);
            IL.MaulingRockZoneManager.FireRock += Utils.HookIL(MaulingRockZoneManager_FireRock);
            IL.RoR2.FogDamageController.MyFixedUpdate += Utils.HookIL(FogDamageController_MyFixedUpdate);
            On.RoR2.LightningStrikeInstance.Initialize += LightningStrikeInstance_Initialize;
            IL.RoR2.CharacterBody.InflictLavaDamage += Utils.HookIL(CharacterBody_InflictLavaDamage);
        }

        private static void CharacterBody_InflictLavaDamage(ILCursor c)
        {
            c.GotoLast(x => x.MatchStfld<DamageInfo>(nameof(DamageInfo.damageType)));
            c.EmitDelegate<DamageTypeComboModifier>(AddDamageType);
        }

        private static bool LightningStrikeInstance_Initialize(On.RoR2.LightningStrikeInstance.orig_Initialize orig, LightningStrikeInstance self, UnityEngine.Vector3 _impactPosition, BlastAttack _blastInfo, float _impactDelay, bool _isIndependentOfStorm)
        {
            var success = orig(self, _impactPosition, _blastInfo, _impactDelay, _isIndependentOfStorm);

            if (!_isIndependentOfStorm)
            {
                self.blastAttack?.AddModdedDamageType(DamageType);
            }

            return success;
        }

        private static void FogDamageController_MyFixedUpdate(ILCursor c)
        {
            // TODO: move this first bit to another class
            ILLabel? label = null;
            int bodyVariableNumber = -1;
            c.GotoNext(MoveType.AfterLabel,
                x => x.MatchLdloc(out bodyVariableNumber),
                x => x.MatchCallvirt<CharacterBody>($"get_{nameof(CharacterBody.IsDrone)}"),
                x => x.MatchBrtrue(out label));
            c.Emit(OpCodes.Ldloc_S, (byte)bodyVariableNumber);
            c.EmitDelegate<Func<CharacterBody, bool>>(key => key.isBoss);
            c.Emit(OpCodes.Brtrue_S, label);

            int variableNumber = -1;
            c.GotoLast(x => x.MatchLdloc(out variableNumber), x => x.MatchStfld<DamageInfo>(nameof(DamageInfo.damageType)));
            c.Index++;
            c.EmitDelegate<DamageTypeComboModifier>(AddDamageType);
            c.Emit(OpCodes.Dup);
            c.Emit(OpCodes.Stloc_S, (byte)variableNumber);
        }

        private static void MaulingRockZoneManager_FireRock(ILCursor c)
        {
            c.GotoNext(x => x.MatchCallvirt<ProjectileManager>(nameof(ProjectileManager.FireProjectile)));
            c.Emit(OpCodes.Dup);
            c.EmitDelegate<Action<FireProjectileInfo>>(self =>
            {
                var damageTypeOverride = self.damageTypeOverride ?? new DamageTypeCombo();
                DamageAPI.AddModdedDamageType(ref damageTypeOverride, DamageType);
                self.damageTypeOverride = damageTypeOverride;
            });
        }

        private static void ExplosivePotDeath_Explode(ILCursor c)
        {
            c.GotoNext(x => x.MatchCallvirt<BlastAttack>(nameof(BlastAttack.Fire)));
            c.Emit(OpCodes.Dup);
            c.EmitDelegate<Action<BlastAttack>>(self => self.AddModdedDamageType(DamageType));
        }

        private void HealthComponent_TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo)
        {
            if (self.body.teamComponent.teamIndex == TeamIndex.Player && damageInfo.HasModdedDamageType(Content.DamageTypes.EnvironmentalHazard) && this.IsEnabled(self))
            {
                damageInfo.damage *= damageMultiplier;
            }

            orig(self, damageInfo);
        }
    }
}