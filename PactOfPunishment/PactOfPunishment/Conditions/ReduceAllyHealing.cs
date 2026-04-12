using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace PactOfPunishment.Conditions
{
    public sealed class ReduceAllyHealing : DefaultConditionDef
    {
        private const float healingReductionPerRank = 0.25f;

        public override int MaxRank => 4;

        public override string Description => string.Format(base.Description, Utils.Percent(healingReductionPerRank));

        public override void Init()
        {
            IL.RoR2.HealthComponent.Heal += Utils.HookIL(HealthComponent_Heal);
            IL.RoR2.HealthComponent.ServerFixedUpdate += this.HealthComponent_ServerFixedUpdate;
            IL.RoR2.HealthComponent.AddBarrier += Utils.HookIL(HealthComponent_AddBarrier);
            IL.RoR2.CharacterBody.RecalculateStats += Utils.HookIL(CharacterBody_RecalculateStats);
            On.RoR2.CharacterMaster.Awake += this.CharacterMaster_Awake;
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
            c.EmitDelegate<Func<float, HealthComponent, float>>(this.ModifyHealing);
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
            c.EmitDelegate<Func<float, HealthComponent, float>>(this.ModifyHealing);
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
            if (self.body.teamComponent.teamIndex == TeamIndex.Player)
            {
                float multiplier = 0.5f;

                int rank = this.GetRank(self);

                if (rank > 0)
                {
                    multiplier *= Mathf.Max(0, 1 - healingReductionPerRank * rank);
                }

                return amount * multiplier;
            }

            return amount;
        }

        [RequireComponent(typeof(CharacterMaster))]
        public class HealthRefreshController : MonoBehaviour
        {
            private CharacterMaster master;

            private float? cachedHealth;

            public void Awake()
            {
                this.master = this.GetComponent<CharacterMaster>();
                this.master.onBodyStart += this.Master_onBodyStart;
                this.master.onBodyDestroyed += this.Master_onBodyDestroyed;
            }

            private void Master_onBodyDestroyed(CharacterBody body)
            {
                this.cachedHealth = body.healthComponent.health;
            }

            private void Master_onBodyStart(CharacterBody body)
            {
                if (!this.cachedHealth.HasValue)
                {
                    return;
                }

                float targetHealth = body.healthComponent.health;

                if (targetHealth > this.cachedHealth)
                {
                    body.healthComponent.health = this.cachedHealth.Value;
                    body.healthComponent.Heal(targetHealth - body.healthComponent.health, default, false);
                }

                this.cachedHealth = null;
            }
        }
    }
}