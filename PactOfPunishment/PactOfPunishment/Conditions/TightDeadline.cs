using HG;
using RoR2;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace PactOfPunishment.Conditions
{
    public sealed class TightDeadline : ConditionDef // TODO: visual timer indication, testing
    {
        private float damagePerSecond = 6;

        public override int MaxRank => 3;

        public override int GetHeatForRank(int rank) => rank;

        public override void Init()
        {
            On.RoR2.InfiniteTowerRun.Start += this.InfiniteTowerRun_Start;
            On.RoR2.InfiniteTowerRun.OnServerStageBegin += this.InfiniteTowerRun_OnServerStageBegin;
        }

        private void InfiniteTowerRun_Start(On.RoR2.InfiniteTowerRun.orig_Start orig, InfiniteTowerRun self)
        {
            if (this.GetRank(self) > 0)
            {
                this.SetupBehavior(self.EnsureComponent<TightDeadlineBehavior>());
            }

            orig(self);
        }

        private void InfiniteTowerRun_OnServerStageBegin(On.RoR2.InfiniteTowerRun.orig_OnServerStageBegin orig, InfiniteTowerRun self, Stage stage)
        {
            if (self.TryGetComponent<TightDeadlineBehavior>(out var behavior))
            {
                behavior.AddTime(this.GetTimeLimitPerRegion(self));
            }

            orig(self, stage);
        }

        private void SetupBehavior(TightDeadlineBehavior behavior)
        {
            behavior.getDamagePerSecond = () => this.damagePerSecond;
        }

        private TimeSpan GetTimeLimitPerRegion(UnityEngine.Object context) => TimeSpan.FromMinutes(12 - this.GetRank(context));

        public class TightDeadlineBehavior : MonoBehaviour
        {
            public TimeSpan timerTotal;

            public Func<float>? getDamagePerSecond;

            private float damageTimer;

            private float tickPeriodSeconds = 0.2f;

            public float GetTimeRemaining(Run run) => (float)this.timerTotal.TotalSeconds - run.GetRunStopwatch();

            public void FixedUpdate()
            {
                if (NetworkServer.active)
                {
                    this.ServerFixedUpdate(Time.fixedDeltaTime);
                }
            }

            internal void AddTime(TimeSpan timeSpan)
            {
                this.timerTotal += timeSpan;
            }

            private void ServerFixedUpdate(float deltaTime)
            {
                this.damageTimer += deltaTime;

                while (this.damageTimer > this.tickPeriodSeconds)
                {
                    this.damageTimer -= this.tickPeriodSeconds;
                    float timeRemaining = this.GetTimeRemaining(Run.instance);

                    if (timeRemaining <= 0)
                    {
                        foreach (var body in TeamComponent.GetTeamMembers(TeamIndex.Player).Select(x => x.body))
                        {
                            if (!body || !body.healthComponent)
                            {
                                continue;
                            }

                            float damage = this.getDamagePerSecond!() * this.tickPeriodSeconds;

                            if (damage > 0f)
                            {
                                DamageTypeCombo damageType = new DamageTypeCombo(DamageType.BypassArmor | DamageType.BypassBlock, DamageTypeExtended.DamageField, DamageSource.Hazard); // TODO: evaluate these
                                body.healthComponent.TakeDamage(new DamageInfo
                                {
                                    damage = damage,
                                    position = body.corePosition,
                                    damageType = damageType,
                                    damageColorIndex = DamageColorIndex.Bleed
                                });
                            }
                        }
                    }
                }
            }
        }
    }
}