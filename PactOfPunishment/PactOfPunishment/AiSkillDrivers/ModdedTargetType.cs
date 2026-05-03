using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.AiSkillDrivers
{
    public class ModdedTargetType
    {
        private static readonly Dictionary<int, Func<BaseAI, BaseAI.Target?>> registered = new Dictionary<int, Func<BaseAI, BaseAI.Target?>>();

        private static int maxKnownIndex = Enum.GetValues(typeof(AISkillDriver.TargetType)).Cast<int>().Max();

        public static AISkillDriver.TargetType Register(Func<BaseAI, BaseAI.Target?> func)
        {
            int index = ++maxKnownIndex;
            registered[index] = func;
            return (AISkillDriver.TargetType)index;
        }

        internal static bool TryGetTarget(BaseAI ai, AISkillDriver aiSkillDriver, out BaseAI.Target? target)
        {
            if (registered.TryGetValue((int)aiSkillDriver.moveTargetType, out var func))
            {
                target = func(ai);
                return true;
            }

            target = null;
            return false;
        }
    }
}