using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PactOfPunishment.Conditions
{
    public sealed class DamageControl : DefaultConditionDef
    {
        public override int MaxRank => 2;

        public override void Init()
        {
            CharacterBody.onBodyStartGlobal += this.CharacterBody_onBodyStartGlobal;
            Content.Buffs.ShieldedHealth = Utils.AddStatusEffect(buffDef =>
            {
                buffDef.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/BearVoid/texBuffBearVoidReady.tif").WaitForCompletion();
                buffDef.buffColor = new Color(0.682f, 0.422f, 0.821f);
                buffDef.canStack = true;
            });
            IL.RoR2.HealthComponent.TakeDamageProcess += this.HealthComponent_TakeDamageProcess;

            // TempVisualEffectAPI.AddTemporaryVisualEffect(CharacterBody.AssetReferences.bearVoidTempEffectPrefab,
            // x => x.HasBuff(Content.Buffs.ShieldedHealth)); // TODO: enable, choose, fix and test
            // visual effect
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody body)
        {
            int rank = this.GetRank(body);

            if (Utils.IsFoe(body) && rank > 0)
            {
                body.SetBuffCount(Content.Buffs.ShieldedHealth.buffIndex, rank);
            }
        }

        private void HealthComponent_TakeDamageProcess(ILContext il)
        {
            var c = new ILCursor(il);
            int flagVariableNumber = -1;
            c.GotoNext(MoveType.AfterLabel,
                x => x.MatchLdloc(out flagVariableNumber),
                x => x.MatchBrtrue(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.body)),
                x => x.MatchLdsfld(typeof(DLC1Content.Buffs), nameof(DLC1Content.Buffs.BearVoidReady)),
                x => x.MatchCallvirt<CharacterBody>(nameof(CharacterBody.HasBuff)),
                x => x.MatchBrfalse(out _));
            c.Emit(OpCodes.Ldloc_S, (byte)flagVariableNumber);
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Action<bool, HealthComponent, DamageInfo>>((flag, self, damageInfo) =>
            {
                if (!flag && self.body.HasBuff(Content.Buffs.ShieldedHealth) && damageInfo.damage > 0f && !damageInfo.rejected)
                {
                    EffectData effectData = new EffectData
                    {
                        origin = damageInfo.position,
                        rotation = Util.QuaternionSafeLookRotation((damageInfo.force != Vector3.zero) ? damageInfo.force : UnityEngine.Random.onUnitSphere)
                    };
                    EffectManager.SpawnEffect(HealthComponent.AssetReferences.bearVoidEffectPrefab, effectData, transmit: true); // TODO: this seems to make a squeaky toy sound, which I don't remember noticing before from monsters. Check with AeroIt, and probably change the effect.
                    damageInfo.rejected = true;
                    self.body.RemoveBuff(Content.Buffs.ShieldedHealth);
                }
            });
        }
    }
}