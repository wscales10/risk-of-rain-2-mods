using System;
using UnityEngine;
using UnityEngine.Networking;

namespace PactOfPunishment
{
    public class RateLimiter : MonoBehaviour
    {
        public Func<bool>? doThing;

        public float minimumInterval = 0.5f;

        private float timer;

        public void TryDoThing()
        {
            if (!NetworkServer.active)
            {
                return;
            }

            if (this.timer <= 0 && this.doThing?.Invoke() == true)
            {
                this.timer = this.minimumInterval;
            }
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
            if (this.timer > 0) { this.timer -= deltaTime; }
        }
    }
}