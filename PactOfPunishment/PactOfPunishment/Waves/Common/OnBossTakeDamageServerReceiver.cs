using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Common
{
    public sealed class OnBossTakeDamageServerReceiver : MonoBehaviour, IOnTakeDamageServerReceiver
    {
        public EntityStateMachine? stateMachine;

        public void OnTakeDamageServer(DamageReport damageReport)
        {
            if (this.stateMachine?.state is IOnBossTakeDamageReceiver receiver)
            {
                receiver.OnBossDamageTaken();
            }
        }
    }
}