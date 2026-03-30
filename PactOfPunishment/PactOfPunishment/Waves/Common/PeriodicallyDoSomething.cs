using System;
using UnityEngine;
using UnityEngine.Networking;

namespace PactOfPunishment.Waves.Common
{
    public class DoSomethingAtFixedRate : PeriodicallyDoSomething
    {
        public float interval;

        protected override bool ShouldEnableTimer()
        {
            return this.interval > 0;
        }

        protected override float GetNextInterval()
        {
            return this.interval;
        }
    }

    public class DoSomethingAtVariableRate : PeriodicallyDoSomething
    {
        public float minInterval;

        public float maxInterval;

        public Xoroshiro128Plus? rng;

        protected override bool ShouldEnableTimer()
        {
            return this.minInterval > 0 && this.maxInterval >= this.minInterval && !(this.rng is null);
        }

        protected override float GetNextInterval()
        {
            return this.rng!.RangeFloat(this.minInterval, this.maxInterval);
        }
    }

    public abstract class PeriodicallyDoSomething : MonoBehaviour
    {
        public Action? doSomething;

        private float timer;

        protected abstract bool ShouldEnableTimer();

        protected abstract float GetNextInterval();

        protected virtual void OnEnable()
        {
            this.timer = 0;
        }

        private void Update()
        {
            if (NetworkServer.active)
            {
                this.ServerUpdate(Time.deltaTime);
            }
        }

        private void ServerUpdate(float deltaTime)
        {
            if (this.doSomething is null || !this.ShouldEnableTimer())
            {
                return;
            }

            this.timer -= deltaTime;

            if (this.timer <= 0)
            {
                try
                {
                    this.doSomething?.Invoke();
                }
                finally
                {
                    this.timer = this.GetNextInterval();
                }
            }
        }
    }
}