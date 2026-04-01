using RoR2;
using UnityEngine;

namespace PactOfPunishment
{
    [RequireComponent(typeof(CombatDirector))]
    public class CombatDirectorInitialDelay : MonoBehaviour
    {
        public float StartDelay;

        public float Timer { get; private set; }

        public void OnEnable()
        {
            this.Timer = this.StartDelay;
        }

        public void Update()
        {
            this.ManagedUpdate(Time.deltaTime);
        }

        public void Skip()
        {
            this.Timer = 0;
        }

        private void ManagedUpdate(float deltaTime)
        {
            if (this.Timer > 0)
            {
                this.Timer -= deltaTime;
            }
        }
    }
}