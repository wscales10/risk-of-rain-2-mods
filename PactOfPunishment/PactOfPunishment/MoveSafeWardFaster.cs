using EntityStates.InfiniteTowerSafeWard;
using HG;
using MonoMod.Cil;
using RoR2;
using System;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment
{
    public class MoveSafeWardFaster : Module
    {
        public override void Init()
        {
            IL.EntityStates.InfiniteTowerSafeWard.Travelling.FixedUpdate += Utils.HookIL(Travelling_FixedUpdate);
            On.RoR2.InfiniteTowerRun.SpawnSafeWard += this.InfiniteTowerRun_SpawnSafeWard;
        }

        private static void Travelling_FixedUpdate(ILCursor c)
        {
            c.GotoNext(x => x.MatchLdfld<Travelling>(nameof(Travelling.travelSpeed)));
            c.Remove();
            c.EmitDelegate<Func<Travelling, float>>(state => state.safeWardController.GetComponent<SafeWardTravelSpeedController>().Speed);
        }

        private void InfiniteTowerRun_SpawnSafeWard(On.RoR2.InfiniteTowerRun.orig_SpawnSafeWard orig, InfiniteTowerRun self, InteractableSpawnCard spawnCard, DirectorPlacementRule placementRule)
        {
            orig(self, spawnCard, placementRule);

            if (self.safeWardController)
            {
                self.safeWardController.EnsureComponent<SafeWardTravelSpeedController>();
            }
        }

        [RequireComponent(typeof(InfiniteTowerSafeWardController))]
        public class SafeWardTravelSpeedController : MonoBehaviour
        {
            private float acceleration = 20;

            private float targetSpeed;

            private InfiniteTowerSafeWardController safeWardController;

            private float? speed;

            public float Speed => this.speed ?? 0;

            public void Awake()
            {
                this.safeWardController = this.GetComponent<InfiniteTowerSafeWardController>();
            }

            public void FixedUpdate()
            {
                this.ManagedFixedUpdate(Time.fixedDeltaTime);
            }

            private float GetTargetSpeed(Travelling state)
            {
                float? min = null;

                foreach (var x in PlayerCharacterMasterController.instances.Select(x => (x?.body)).Where(x => x?.healthComponent?.alive == true).Select(body => (body, body!.teamComponent?.transform?.position)).Where(x => x.position.HasValue))
                {
                    float bonus = this.GetTravelSpeedBonus(state, Mathf.Max(0, Util.Vector3XZToVector2XY(x.body!.characterMotor.velocity).magnitude * 1.5f - state.travelSpeed), x.position!.Value);

                    if (min is null || bonus < min)
                    {
                        min = bonus;
                    }
                }

                return state.travelSpeed + (min ?? 0);
            }

            private float GetTravelSpeedBonus(Travelling state, float maxBonus, Vector3 playerPosition)
            {
                var zone = this.safeWardController._safeZone;
                Vector3 vector = playerPosition - zone.transform.position;
                vector.y = 0;
                Vector3 onNormal = state.transform.forward;
                onNormal.y = 0;

                float projection = Vector3.Dot(vector, onNormal.normalized);

                if (Mathf.Abs(projection) > zone.radius)
                {
                    return 0;
                }

                return Mathf.Max(0, maxBonus * (projection / zone.radius + 1) * 0.5f);
            }

            private void ManagedFixedUpdate(float deltaTime)
            {
                switch (this.safeWardController.wardStateMachine.state)
                {
                    case Travelling travelling:
                        this.targetSpeed = this.GetTargetSpeed(travelling);

                        if (this.speed.HasValue)
                        {
                            this.speed = Mathf.MoveTowards(this.speed.Value, this.targetSpeed, deltaTime * this.acceleration);
                        }
                        else
                        {
                            this.speed = this.targetSpeed;
                        }

                        break;

                    default:
                        this.speed = null;
                        break;
                }
            }
        }
    }
}