using UnityEngine;

namespace AssortedExperiments.Items
{
    public class RegenTickController : MonoBehaviour
    {
        public float Timer { get; private set; }

        public float Interval { get; set; } = 0.2f;

        public void TickDown(float deltaTime) => this.Timer -= deltaTime;

        public void Reset() => this.Timer = this.Interval;
    }
}
