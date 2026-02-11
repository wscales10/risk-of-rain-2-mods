using System;
using UnityEngine;
using UnityEngine.Networking;

namespace PactOfPunishment.Waves
{
    public class PeriodicallyDoSomething : MonoBehaviour
    {
        public float interval;

        public Action? doSomething;

        private float timer;

        private void Update()
        {
            if (NetworkServer.active)
            {
                this.ServerUpdate(Time.deltaTime);
            }
        }

        private void ServerUpdate(float deltaTime)
        {
            if (this.doSomething is null || this.interval <= 0)
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
                    this.timer = this.interval;
                }
            }
        }
    }
}