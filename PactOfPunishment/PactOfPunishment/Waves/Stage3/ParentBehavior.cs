using RoR2;
using System;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage3
{
    // TODO: choose a better name
    public class ParentBehavior : MonoBehaviour, ILifeBehavior
    {
        public Action<ParentBehavior>? onDeathStart;

        public void OnDeathStart()
        {
            this.onDeathStart?.Invoke(this);
        }
    }
}
