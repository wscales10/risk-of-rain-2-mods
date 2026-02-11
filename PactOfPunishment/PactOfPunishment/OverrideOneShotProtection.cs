using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment
{
    public class OverrideOneShotProtection : Module
    {
        private readonly HashSet<OneShotProtectionSource> sources = new HashSet<OneShotProtectionSource>();

        public static OverrideOneShotProtection Instance { get; } = new OverrideOneShotProtection();
        
        private OverrideOneShotProtection()
        {
            this.GiveOneShotProtectionToPlayers = this.AddSource(new OneShotProtectionSource(body => body.isPlayerControlled));
        }

        public OneShotProtectionSource GiveOneShotProtectionToPlayers { get; }

        public override void Init()
        {
            IL.RoR2.CharacterBody.RecalculateStats += this.CharacterBody_RecalculateStats;
        }

        public OneShotProtectionSource AddSource(OneShotProtectionSource source)
        {
            this.sources.Add(source);
            return source;
        }

        private void CharacterBody_RecalculateStats(ILContext il)
        {
            var c = new ILCursor(il);
            c.GotoNext(x => x.MatchCall<CharacterBody>($"set_{nameof(CharacterBody.hasOneShotProtection)}"));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<bool, CharacterBody, bool>>((value, self) =>
            {
                return this.sources.Any(x => x.ShouldGrantOneShotProtection(self)); // TODO: should I use the original value rather than replicating the logic above?
            });
        }
    }

    public class OneShotProtectionSource
    {
        private readonly List<Func<Func<CharacterBody, bool>, CharacterBody, bool>> overrides = new List<Func<Func<CharacterBody, bool>, CharacterBody, bool>>();

        private readonly Func<CharacterBody, bool> defaultBehavior;

        public OneShotProtectionSource(Func<CharacterBody, bool> defaultBehavior)
        {
            this.defaultBehavior = defaultBehavior;
            this.ShouldGrantOneShotProtection = this.defaultBehavior;
        }

        public Func<CharacterBody, bool> ShouldGrantOneShotProtection { get; private set; }

        public void Override(Func<Func<CharacterBody, bool>, CharacterBody, bool> @override)
        {
            this.overrides.Add(@override);
            this.ShouldGrantOneShotProtection = this.Build();
        }

        private Func<CharacterBody, bool> Build()
        {
            Func<CharacterBody, bool> current = this.defaultBehavior;

            foreach (var @override in this.overrides)
            {
                current = body => @override(current, body);
            }

            return current;
        }
    }
}