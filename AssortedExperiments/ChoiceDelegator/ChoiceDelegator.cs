using BepInEx;
using MonoMod.Cil;
using RoR2;
using System;

namespace ChoiceDelegator
{

    [BepInPlugin("com.woodyscales.choicedelegator", "Choice Delegator", "1.0.0")]
    public class ChoiceDelegator : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.PickupDropletController.CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3 += this.PickupDropletController_CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3;

            // Test handler that always gives BleedOnHit item
            PickupChoiceManager.ChoosePickup += PickupChoiceManager_ChoosePickup;
        }

        private static void PickupChoiceManager_ChoosePickup(object sender, ChoosePickupEventArgs e)
        {
            e.Pickup = new UniquePickup(PickupCatalog.FindPickupIndex(RoR2Content.Items.BleedOnHit.itemIndex));
            e.Handled = true;
        }

        private void PickupDropletController_CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3(On.RoR2.PickupDropletController.orig_CreatePickupDroplet_CreatePickupInfo_Vector3_Vector3 orig, RoR2.GenericPickupController.CreatePickupInfo pickupInfo, UnityEngine.Vector3 position, UnityEngine.Vector3 velocity)
        {
            this.ModifyPickupInfo(pickupInfo);
            orig(pickupInfo, position, velocity);
        }

        private void ModifyPickupInfo(RoR2.GenericPickupController.CreatePickupInfo pickupInfo)
        {
            this.Logger.LogDebug($"Creating pickup droplet at {DateTime.UtcNow.ToLongTimeString()}");
            ChoosePickupEventArgs e = new ChoosePickupEventArgs(pickupInfo);
            PickupChoiceManager.OnChoosePickup(this, e);
            pickupInfo.pickup = e.Pickup;
        }
    }
}