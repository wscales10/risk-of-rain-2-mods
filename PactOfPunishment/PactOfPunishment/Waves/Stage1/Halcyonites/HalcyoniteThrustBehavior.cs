using EntityStates.Halcyonite;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage1.Halcyonites
{
    [RequireComponent(typeof(CharacterBody))]
    public class HalcyoniteThrustBehavior : MonoBehaviour
    {
        public Func<float>? getDesiredDistance;

        private readonly List<SwipeTimer> swipeTimers = new List<SwipeTimer>();

        private CharacterBody body;

        public event Action? OnThrust;

        public void Awake()
        {
            this.body = this.GetComponent<CharacterBody>();
        }

        public void OnSwipe(GoldenSwipe state)
        {
            if (state.isAuthority && state.characterBody && state.characterBody.characterMotor)
            {
                this.swipeTimers.Add(new SwipeTimer { Timer = state.duration * 0.35f, State = state });
            }
        }

        public void FixedUpdate()
        {
            this.ManagedFixedUpdate(Time.fixedDeltaTime);
        }

        private void ManagedFixedUpdate(float deltaTime)
        {
            for (int i = this.swipeTimers.Count - 1; i >= 0; i--)
            {
                this.UpdateSwipeTimer(i, deltaTime);
            }
        }

        private void UpdateSwipeTimer(int index, float deltaTime)
        {
            var state = this.swipeTimers[index].State;

            if (state.outer?.state != state) // TODO: this check means that we don't need a list of timers
            {
                this.swipeTimers.RemoveAt(index);
                return;
            }

            this.swipeTimers[index].Timer -= deltaTime;

            if (this.swipeTimers[index].Timer >= 0)
            {
                return;
            }

            this.swipeTimers.RemoveAt(index);
            this.OnThrust?.Invoke();

            if (this.getDesiredDistance == null)
            {
                Debug.LogError($"{this.name}.{nameof(this.getDesiredDistance)} == null.");
            }
            else
            {
                var desiredDistance = this.getDesiredDistance();
                Debug.Log($"{this.name}.{nameof(this.getDesiredDistance)} returned {desiredDistance}.");
                float xSpeed = Trajectory.CalculateInitialYSpeedForHeight(desiredDistance, -this.body.acceleration);
                float mass = this.body.characterMotor ? this.body.characterMotor.mass : 1f;
                this.body.characterMotor.ApplyForce(xSpeed * mass * (this.body.inputBank ? this.body.inputBank.aimDirection : this.body.transform.forward));
            }
        }

        private sealed class SwipeTimer
        {
            public float Timer;

            public GoldenSwipe State;
        }
    }
}