using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using PactOfPunishment.Conditions;
using RoR2;
using System;
using UnityEngine;

namespace PactOfPunishment
{
    public enum HealthGainType
    {
        Heal,

        Shield,

        Barrier
    }

    public interface IModifyHealthGain
    {
        void ModifyHealthGain(HealthGainModificationArgs args);
    }

    public class HealthGainModificationArgs
    {
        public HealthGainModificationArgs(HealthComponent healthComponent, HealthGainType healthGainType)
        {
            this.HealthComponent = healthComponent;
            this.HealthGainType = healthGainType;
        }

        public HealthComponent HealthComponent { get; }

        public float HealthGainMultiplier { get; set; } = 1;

        public HealthGainType HealthGainType { get; }
    }

    public class ModifyHealthGain : Module
    {
        private static float baseHealingMultiplier = 0.5f;

        private static float baseShieldsMultiplier = 0.5f;

        private static float baseBarrierMultiplier = 1f;

        public override void Init()
        {
            IL.RoR2.HealthComponent.Heal += Utils.HookIL(HealthComponent_Heal);
            IL.RoR2.HealthComponent.ServerFixedUpdate += this.HealthComponent_ServerFixedUpdate;
            IL.RoR2.HealthComponent.AddBarrier += Utils.HookIL(HealthComponent_AddBarrier);
            IL.RoR2.CharacterBody.RecalculateStats += Utils.HookIL(CharacterBody_RecalculateStats);
            On.RoR2.CharacterMaster.Awake += this.CharacterMaster_Awake;
            On.RoR2.CharacterBody.Awake += this.CharacterBody_Awake;
            On.RoR2.HealthComponent.RechargeShield += this.HealthComponent_RechargeShield;
        }

        private static void CharacterBody_RecalculateStats(ILCursor c)
        {
            int oldMaxHealthVariableNumber = -1;
            int maxHealthIncreaseVariableNumber = -1;
            c.RemoveMatch(

                // if (maxHealthIncrease > 0f)
                x => x.MatchLdloc(out maxHealthIncreaseVariableNumber),
                x => x.MatchLdcR4(0f),
                x => x.MatchBleUn(out _),

                // Heal(...)
                x => x.MatchLdarg(0),
                x => x.MatchCall<CharacterBody>($"get_{nameof(CharacterBody.healthComponent)}"),
                x => x.MatchLdloc(out _),
                x => x.MatchLdloca(out _),
                x => x.MatchInitobj<ProcChainMask>(),
                x => x.MatchLdloc(out _),
                x => x.MatchLdcI4(0),
                x => x.MatchCallvirt<HealthComponent>(nameof(HealthComponent.Heal)),
                x => x.MatchPop(),
                x => x.MatchBr(out _),

                // else if (health > maxHealth)
                x => x.MatchLdarg(0),
                x => x.MatchCall<CharacterBody>($"get_{nameof(CharacterBody.healthComponent)}"),
                x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.health)),
                x => x.MatchLdarg(0),
                x => x.MatchCall<CharacterBody>($"get_{nameof(CharacterBody.maxHealth)}"),
                x => x.MatchBleUn(out _),

                // Networkhealth = Mathf.Max(...)
                x => x.MatchLdarg(0),
                x => x.MatchCall<CharacterBody>($"get_{nameof(CharacterBody.healthComponent)}"),
                x => x.MatchLdarg(0),
                x => x.MatchCall<CharacterBody>($"get_{nameof(CharacterBody.healthComponent)}"),
                x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.health)),
                x => x.MatchLdloc(out _),
                x => x.MatchAdd(),
                x => x.MatchLdarg(0),
                x => x.MatchCall<CharacterBody>($"get_{nameof(CharacterBody.maxHealth)}"),
                x => x.MatchCall<Mathf>(nameof(Mathf.Max)),
                x => x.MatchCall<HealthComponent>($"set_{nameof(HealthComponent.Networkhealth)}")
            );

            c.FindPrev(out _,
                x => x.MatchLdarg(0),
                x => x.MatchCall<CharacterBody>($"get_{nameof(CharacterBody.maxHealth)}"),
                x => x.MatchLdloc(out oldMaxHealthVariableNumber),
                x => x.MatchSub(),
                x => x.MatchStloc(maxHealthIncreaseVariableNumber)
            );

            c.MoveAfterLabels();
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc_S, (byte)oldMaxHealthVariableNumber);
            c.EmitDelegate<Action<CharacterBody, float>>((self, oldMaxHealth) => self.healthComponent.Networkhealth = self.maxHealth * (self.healthComponent.health / oldMaxHealth));

            int oldMaxShieldVariableNumber = -1;
            int maxShieldIncreaseVariableNumber = -1;
            c.RemoveMatch(

                // if (num127 > 0f)
                x => x.MatchLdloc(out maxShieldIncreaseVariableNumber),
                x => x.MatchLdcR4(0f),
                x => x.MatchBleUn(out _),

                // RechargeShield(num127)
                x => x.MatchLdarg(0),
                x => x.MatchCall<CharacterBody>($"get_{nameof(CharacterBody.healthComponent)}"),
                x => x.MatchLdloc(out _),
                x => x.MatchCallvirt<HealthComponent>(nameof(HealthComponent.RechargeShield)),
                x => x.MatchBr(out _),

                // else if (shield > maxShield)
                x => x.MatchLdarg(0),
                x => x.MatchCall<CharacterBody>($"get_{nameof(CharacterBody.healthComponent)}"),
                x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.shield)),
                x => x.MatchLdarg(0),
                x => x.MatchCall<CharacterBody>($"get_{nameof(CharacterBody.maxShield)}"),
                x => x.MatchBleUn(out _),

                // Networkshield = Mathf.Max(...)
                x => x.MatchLdarg(0),
                x => x.MatchCall<CharacterBody>($"get_{nameof(CharacterBody.healthComponent)}"),
                x => x.MatchLdarg(0),
                x => x.MatchCall<CharacterBody>($"get_{nameof(CharacterBody.healthComponent)}"),
                x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.shield)),
                x => x.MatchLdloc(out _),
                x => x.MatchAdd(),
                x => x.MatchLdarg(0),
                x => x.MatchCall<CharacterBody>($"get_{nameof(CharacterBody.maxShield)}"),
                x => x.MatchCall<UnityEngine.Mathf>(nameof(UnityEngine.Mathf.Max)),
                x => x.MatchCall<HealthComponent>($"set_{nameof(HealthComponent.Networkshield)}")
            );

            c.FindPrev(out _,
                x => x.MatchLdarg(0),
                x => x.MatchCall<CharacterBody>($"get_{nameof(CharacterBody.maxShield)}"),
                x => x.MatchLdloc(out oldMaxShieldVariableNumber),
                x => x.MatchSub(),
                x => x.MatchStloc(maxShieldIncreaseVariableNumber)
            );

            c.MoveAfterLabels();
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc_S, (byte)oldMaxShieldVariableNumber);
            c.EmitDelegate<Action<CharacterBody, float>>((self, oldMaxShield) =>
            {
                var curseTracker = self.GetComponent<CursePenaltyTracker>();
                float cachedCursePenalty = curseTracker.CachedCursePenalty;
                curseTracker.CachedCursePenalty = self.cursePenalty;
                self.healthComponent.Networkshield = Mathf.Max(0, Mathf.Min(self.maxShield, (self.healthComponent.shield * cachedCursePenalty + self.maxShield * self.cursePenalty - oldMaxShield * cachedCursePenalty) / self.cursePenalty));
            });
        }

        private void HealthComponent_RechargeShield(On.RoR2.HealthComponent.orig_RechargeShield orig, HealthComponent self, float value)
        {
            orig(self, this.ModifyShields(value, self));
        }

        private void CharacterBody_Awake(On.RoR2.CharacterBody.orig_Awake orig, CharacterBody self)
        {
            orig(self);
            self.gameObject.AddComponent<CursePenaltyTracker>();
        }

        private void CharacterMaster_Awake(On.RoR2.CharacterMaster.orig_Awake orig, CharacterMaster self)
        {
            orig(self);

            if (self.playerCharacterMasterController)
            {
                self.EnsureComponent<HealthRefreshController>();
            }
        }

        private void HealthComponent_AddBarrier(ILCursor c)
        {
            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.barrier)),
                x => x.MatchLdarg(1),
                x => x.MatchAdd(),
                x => x.MatchLdarg(0),
                x => x.MatchCall<HealthComponent>($"get_{nameof(HealthComponent.fullBarrier)}"),
                x => x.MatchCall<Mathf>(nameof(Mathf.Min)),
                x => x.MatchCall<HealthComponent>($"set_{nameof(HealthComponent.Networkbarrier)}")
            );
            c.Index += 4;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<float, HealthComponent, float>>(this.ModifyBarrier);
        }

        private void HealthComponent_ServerFixedUpdate(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(MoveType.After,
                x => x.MatchLdloc(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.body)),
                x => x.MatchCallvirt<CharacterBody>($"get_{nameof(CharacterBody.maxShield)}"),
                x => x.MatchLdcR4(0.5f),
                x => x.MatchMul(),
                x => x.MatchLdarg(1),
                x => x.MatchMul());
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<float, HealthComponent, float>>(this.ModifyShields);
        }

        private void HealthComponent_Heal(ILCursor c)
        {
            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.health)),
                x => x.MatchLdloc(out _),
                x => x.MatchAdd(),
                x => x.MatchCall<HealthComponent>($"set_{nameof(HealthComponent.Networkhealth)}")
            );
            c.Index -= 2;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<float, HealthComponent, float>>(this.ModifyHealing);
        }

        private float ModifyHealing(float amount, HealthComponent self)
        {
            return this.ModifyHealingInternal(amount, self, HealthGainType.Heal);
        }

        private float ModifyShields(float amount, HealthComponent self)
        {
            return this.ModifyHealingInternal(amount, self, HealthGainType.Shield);
        }

        private float ModifyBarrier(float amount, HealthComponent self)
        {
            return this.ModifyHealingInternal(amount, self, HealthGainType.Barrier);
        }

        private float ModifyHealingInternal(float amount, HealthComponent self, HealthGainType healthGainType)
        {
            var args = new HealthGainModificationArgs(self, healthGainType)
            {
                // TODO: depends on team? probably not.
                HealthGainMultiplier = (healthGainType switch
                {
                    HealthGainType.Heal => baseHealingMultiplier,
                    HealthGainType.Shield => baseShieldsMultiplier,
                    HealthGainType.Barrier => baseBarrierMultiplier,
                    _ => throw new ArgumentOutOfRangeException(nameof(healthGainType), healthGainType, null),
                })
            };

            ReduceAllyHealing.Instance?.ModifyHealthGain(args);
            return amount * args.HealthGainMultiplier;
        }

        [RequireComponent(typeof(CharacterBody))]
        public class CursePenaltyTracker : MonoBehaviour
        {
            public float CachedCursePenalty { get; internal set; } = 1;
        }

        [RequireComponent(typeof(CharacterMaster))]
        public class HealthRefreshController : MonoBehaviour
        {
            private CharacterMaster master;

            private float? cachedHealthFraction;

            private float? cachedShieldFraction;

            public void Awake()
            {
                this.master = this.GetComponent<CharacterMaster>();
                this.master.onBodyStart += this.Master_onBodyStart;
                this.master.onBodyDestroyed += this.Master_onBodyDestroyed;
            }

            private void Master_onBodyDestroyed(CharacterBody body)
            {
                float currentBodyHealth = body.healthComponent.health;
                float currentMaxHealth = body.maxHealth;

                if (currentBodyHealth > 0 && currentMaxHealth > 0)
                {
                    this.cachedHealthFraction = currentBodyHealth / currentMaxHealth;
                }
                else
                {
                    this.cachedHealthFraction = null;
                }

                float currentBodyShield = body.healthComponent.shield;
                float currentMaxShield = body.maxShield;

                if (currentBodyHealth > 0 && currentMaxShield > 0)
                {
                    this.cachedShieldFraction = Mathf.Max(0, currentBodyShield) / currentMaxShield;
                }
                else
                {
                    this.cachedShieldFraction = null;
                }
            }

            private void Master_onBodyStart(CharacterBody body)
            {
                if (this.cachedHealthFraction.HasValue)
                {
                    float targetHealth = body.healthComponent.health;
                    float healthFromCache = this.cachedHealthFraction.Value * body.maxHealth;

                    if (targetHealth > healthFromCache)
                    {
                        body.healthComponent.health = healthFromCache;
                        body.healthComponent.Heal(targetHealth - body.healthComponent.health, default, false);
                    }

                    this.cachedHealthFraction = null;
                }

                if (this.cachedShieldFraction.HasValue)
                {
                    float targetShield = body.healthComponent.shield;
                    float shieldFromCache = this.cachedShieldFraction.Value * body.maxShield;

                    if (targetShield > shieldFromCache)
                    {
                        body.healthComponent.shield = shieldFromCache;
                        body.healthComponent.RechargeShield(targetShield - body.healthComponent.shield);
                    }

                    this.cachedShieldFraction = null;
                }
            }
        }
    }
}