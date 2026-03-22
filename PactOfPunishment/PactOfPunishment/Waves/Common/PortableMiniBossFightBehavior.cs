using HG;
using RoR2;
using System;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.Common
{
    public abstract class PortableMiniBossFightBehavior<T> : BossFightBehavior
        where T : PortableMiniBossFightBehavior<T>
    {
        [SerializeField]
        private PortableMiniBossInfo[]? miniBosses;

        public void SetMiniBosses(PortableMiniBossInfo<T>[]? value) => this.miniBosses = value;

        public event Action<bool>? OnSetEnabled;

        protected bool CustomEnabled { get; private set; }

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
            if (this.miniBosses is null)
            {
                Debug.LogError($"{this.GetType().Name}.{nameof(this.miniBosses)} was null.");
                return;
            }

            var miniBossInfo = this.miniBosses.SingleOrDefault(info => body.Is(info.BodyPrefab));

            if (!(miniBossInfo is null))
            {
                body.EnsureComponent<ChiefBossMarker>();
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