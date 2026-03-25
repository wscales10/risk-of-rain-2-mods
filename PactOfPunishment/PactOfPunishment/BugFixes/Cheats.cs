using HarmonyLib;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using static RoR2.Console;

namespace PactOfPunishment.BugFixes
{
    public class Cheats : Module
    {
        public override void Init()
        {
#if DEBUG
            MethodInfo getterMethod = AccessTools.PropertyGetter(typeof(CheatsConVar), nameof(CheatsConVar.boolValue));
            _ = new Hook(getterMethod, AccessTools.DeclaredMethod(this.GetType(), nameof(OnGetBoolValue)));

            MethodInfo setterMethod = AccessTools.PropertySetter(typeof(CheatsConVar), nameof(CheatsConVar.boolValue));
            _ = new Hook(setterMethod, AccessTools.DeclaredMethod(this.GetType(), nameof(OnSetBoolValue)));
#endif
        }

        private static bool OnGetBoolValue(Func<CheatsConVar, bool> orig, CheatsConVar self)
        {
            return sessionCheatsEnabled;
        }

        private static void OnSetBoolValue(Action<CheatsConVar, bool> orig, CheatsConVar self, bool value)
        {
            sessionCheatsEnabled = value;
        }
    }
}