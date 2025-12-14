using BepInEx;

namespace MakeExceptionsGoAway
{
    [BepInPlugin("com.woodyscales.exceptionsbegone", "Exceptions Begone", "1.0.0")]
    public class MakeExceptionsGoAway : BaseUnityPlugin
    {
        private void Awake()
        {
            On.RoR2.AttackSpeedPerNearbyCollider.ReconcileBuffCount += AttackSpeedPerNearbyCollider_ReconcileBuffCount;
        }

        private static void AttackSpeedPerNearbyCollider_ReconcileBuffCount(On.RoR2.AttackSpeedPerNearbyCollider.orig_ReconcileBuffCount orig, RoR2.AttackSpeedPerNearbyCollider self)
        {
            if (self.body is null)
            {
                return;
            }

            orig(self);
        }
    }
}