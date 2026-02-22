using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Common
{
    public class MakeAllDamageNonLethalBehavior : MonoBehaviour, IOnIncomingDamageServerReceiver
    {
        public void OnIncomingDamageServer(DamageInfo damageInfo)
        {
            if (this.enabled)
            {
                damageInfo.damageType |= DamageType.NonLethal;
            }
        }
    }
}