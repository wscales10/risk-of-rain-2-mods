using HG;
using RoR2;
using System;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.Common
{
    public abstract class PortableMiniBossFightBehavior<T> : BossFightBehavior, IOnOff
        where T : PortableMiniBossFightBehavior<T>
    {
        [SerializeField]
        private PortableMiniBossInfo[]? miniBosses;

        public event Action<bool>? OnSetEnabled;

        bool IOnOff.CustomEnabled => this.CustomEnabled;

        protected bool CustomEnabled { get; private set; }

        public void SetMiniBosses(PortableMiniBossInfo<T>[]? value) => this.miniBosses = value;

        public override void Awake()
        {
            base.Awake();
            Utils.DoSomethingWhenLastMainSquadMemberDies(this.CombatDirector.combatSquad, x => this.miniBosses.Any(b => x.GetBody().Is(b.BodyPrefab)), this.OnLastMainBossDefeated);
        }

        public void Enable()
        {
            this.CustomEnabled = true;
            this.ApplyEnabledState();
        }

        public virtual void Disable()
        {
            this.CustomEnabled = false;
            this.ApplyEnabledState();
        }

        public virtual void ApplyEnabledState()
        {
            this.OnSetEnabled?.Invoke(this.CustomEnabled);
        }

        protected override void OnBossSpawnedServer(CharacterBody body)
        {
            if (this.miniBosses == null)
            {
                Debug.LogError($"{this.GetType().Name}.{nameof(this.miniBosses)} was null.");
                return;
            }

            var miniBossInfo = this.miniBosses.SingleOrDefault(info => body.Is(info.BodyPrefab));

            if (!(miniBossInfo == null))
            {
                body.EnsureHasItem(Content.Items.ChiefBossMarker);
                miniBossInfo?.SetupBossBody(body, this);
                this.Enable();
            }
        }

        private void OnLastMainBossDefeated(CharacterMaster master, DamageReport report)
        {
            this.Disable();
        }
    }
}