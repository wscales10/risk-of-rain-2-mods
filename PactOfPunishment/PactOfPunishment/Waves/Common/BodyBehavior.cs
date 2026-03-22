using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Common
{
    [RequireComponent(typeof(CharacterBody))]
    public abstract class BossBodyBehavior : MonoBehaviour
    {
        public CharacterBody Body { get; private set; }

        public void FixedUpdate()
        {
            this.ManagedFixedUpdate(Time.fixedDeltaTime);
        }

        protected virtual void Awake()
        {
            this.Body = this.GetComponent<CharacterBody>();
        }

        protected virtual void ManagedFixedUpdate(float deltaTime)
        {
        }
    }
}