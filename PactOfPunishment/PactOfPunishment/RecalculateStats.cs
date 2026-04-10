using EntityStates;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PactOfPunishment
{
    public class RecalculateStats : Module
    {
        private static readonly Dictionary<CharacterBody, List<Action<RecalculateStatsAPI.StatHookEventArgs>>> dictionary = new Dictionary<CharacterBody, List<Action<RecalculateStatsAPI.StatHookEventArgs>>>();

        private static readonly Dictionary<GenericSkill, List<GetRechargeStockDelegate>> rechargeStockOverrides = new Dictionary<GenericSkill, List<GetRechargeStockDelegate>>();

        private static readonly Dictionary<(EntityStateMachine, Type), Func<EntityState, InterruptPriority>> minimumInterruptPriorityOverrides = new Dictionary<(EntityStateMachine, Type), Func<EntityState, InterruptPriority>>();

        private RecalculateStats()
        {
        }

        public delegate int GetRechargeStockDelegate(int orig, GenericSkill self);

        public static RecalculateStats Instance { get; } = new RecalculateStats();

        public static void Add(CharacterBody body, Action<RecalculateStatsAPI.StatHookEventArgs> action)
        {
            if (!dictionary.TryGetValue(body, out var set))
            {
                set = new List<Action<RecalculateStatsAPI.StatHookEventArgs>>();
                dictionary.Add(body, set);
            }

            if (!set.Contains(action))
            {
                set.Add(action);
            }
        }

        public static void Remove(CharacterBody body, Action<RecalculateStatsAPI.StatHookEventArgs> action)
        {
            if (!dictionary.TryGetValue(body, out var set))
            {
                return;
            }

            set.Remove(action);
        }

        public override void Init()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            MethodInfo getterMethod = AccessTools.PropertyGetter(typeof(GenericSkill), nameof(GenericSkill.rechargeStock));
            _ = new Hook(getterMethod, AccessTools.DeclaredMethod(this.GetType(), nameof(OnGetRechargeStock)));
            IL.RoR2.EntityStateMachine.CanInterruptState += Utils.HookIL(EntityStateMachine_CanInterruptState);
        }

        internal static void OverrideRechargeStock(GenericSkill skill, GetRechargeStockDelegate getRechargeStock)
        {
            if (!rechargeStockOverrides.TryGetValue(skill, out var list))
            {
                list = new List<GetRechargeStockDelegate>();
                rechargeStockOverrides.Add(skill, list);
            }

            if (!list.Contains(getRechargeStock))
            {
                list.Add(getRechargeStock);
            }
        }

        internal static void SetMinimumInterruptPriorityOverride(EntityStateMachine stateMachine, Type entityStateType, Func<EntityState, InterruptPriority> getMinimumInterruptPriority)
        {
            minimumInterruptPriorityOverrides[(stateMachine, entityStateType)] = getMinimumInterruptPriority;
        }

        private static void EntityStateMachine_CanInterruptState(ILCursor c)
        {
            c.GotoNext(MoveType.AfterLabel, x => x.MatchCallvirt<EntityState>(nameof(EntityState.GetMinimumInterruptPriority)));
            c.Remove();
            c.MoveAfterLabels();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<EntityState, EntityStateMachine, InterruptPriority>>(GetMinimumInterruptPriority);
        }

        private static InterruptPriority GetMinimumInterruptPriority(EntityState state, EntityStateMachine stateMachine)
        {
            if (minimumInterruptPriorityOverrides.TryGetValue((stateMachine, state.GetType()), out var func))
            {
                return func(state);
            }

            return state.GetMinimumInterruptPriority();
        }

        private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (dictionary.TryGetValue(sender, out var list))
            {
                foreach (var action in list)
                {
                    action(args);
                }
            }
        }

        private static int OnGetRechargeStock(Func<GenericSkill, int> orig, GenericSkill self)
        {
            int result = orig(self);

            if (rechargeStockOverrides.TryGetValue(self, out var list))
            {
                foreach (var func in list)
                {
                    result = func(result, self);
                }
            }

            return result;
        }
    }
}