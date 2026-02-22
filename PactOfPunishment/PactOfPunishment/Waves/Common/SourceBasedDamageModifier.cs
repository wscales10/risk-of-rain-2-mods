using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Common
{
    public class SourceBasedDamageModifier : MonoBehaviour, IOnIncomingDamageServerReceiver
    {
        [Tooltip("The DamageCoefficient applied to the incoming damageInfo, this gets applied ONLY if the incoming damageInfo's source does NOT match our sourceMask.")]
        public float damageCoeficient = 1f;

        public DamageSource sourceMask;

        public void OnIncomingDamageServer(DamageInfo damageInfo)
        {
            if ((damageInfo.damageType.damageSource & this.sourceMask) > DamageSource.NoneSpecified)
            {
                return;
            }

            damageInfo.damage *= this.damageCoeficient;
        }
    }
}