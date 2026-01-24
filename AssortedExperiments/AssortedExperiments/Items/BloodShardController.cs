using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AssortedExperiments.Items
{
    public class BloodShardController : MonoBehaviour
    {
        private HoldoutZoneController holdoutZoneController;

        private AsyncOperationHandle<GameObject> loadSiphonPrefabWaitHandle;

        private GameObject? siphonObject;

        public int CurrentItemCount => CharacterMaster.readOnlyInstancesList
                    .Where(characterMaster => characterMaster.teamIndex == this.holdoutZoneController.chargingTeam && this.holdoutZoneController.IsBodyInChargingRadius(characterMaster.GetBody()))
                    .Sum(characterMaster => characterMaster.inventory.GetItemCountEffective(Content.Items.BloodShard.itemIndex));

        // Consider smoothing out into a curve, but it's not super important.
        private float ChargeRateBonus => Mathf.Min(this.holdoutZoneController.baseChargeDuration, 60f) / 36f;

        /*[SerializeField]
        public string siphonRootName = "Siphon";*/

        private BloodSiphonNearbyController? SiphonController => this.siphonObject ? this.siphonObject!.GetComponent<BloodSiphonNearbyController>() : null;

        public float GetChargeRateBonus()
        {
            int currentItemCount = this.CurrentItemCount;

            if (currentItemCount > 0)
            {
                return this.ChargeRateBonus * currentItemCount;
            }

            return 0;
        }

        internal static void Filter(ref SphereSearch.SearchData searchData)
        {
            for (int i = searchData.candidatesCount - 1; i >= 0; i--)
            {
                ref SphereSearch.Candidate candidate = ref searchData.GetCandidate(i);
                var characterBody = GetCharacterBodyFromHurtBox(candidate.hurtBox);

                if (characterBody)
                {
                    var inventory = characterBody!.inventory;

                    if (inventory && inventory.GetItemCountEffective(Content.Items.BloodShard) > 0)
                    {
                        continue;
                    }

                    var minionMasterInventory = characterBody.Then(x => x.master).Then(x => x.minionOwnership).Then(x => x.ownerMaster).Then(x => x.inventory);

                    if (minionMasterInventory && minionMasterInventory!.GetItemCountEffective(Content.Items.BloodShard) > 0)
                    {
                        continue;
                    }
                }

                searchData.RemoveCandidate(i);
            }
        }

        internal static void Sort(List<HurtBox> list)
        {
            var sorted = list.OrderByDescending(x => {
                var inventory = x.healthComponent.Then(x => x.body).Then(x => x.inventory);
                if(!inventory)
                {
                    return 0;
                }

                return inventory!.GetItemCountEffective(Content.Items.BloodShard);
            }).ToList();

            list.Clear();
            list.AddRange(sorted);
        }

        internal void UpdateHealthFractionCoefficients()
        {
            var siphonController = this.SiphonController;

            if (!siphonController)
            {
                return;
            }

            var healthFractionCoefficient = .02f * this.GetComponent<LunarShardsController>().HoldoutZoneChargeRateMultiplier;
            siphonController!.minHealthFractionCoefficient = healthFractionCoefficient;
            siphonController.maxHealthFractionCoefficient = healthFractionCoefficient;
        }

        private static CharacterBody? GetCharacterBodyFromHurtBox(HurtBox? hurtBox)
        {
            if (!hurtBox)
            {
                return null;
            }

            var healthComponent = hurtBox!.healthComponent;

            if (!healthComponent)
            {
                return null;
            }

            return healthComponent.body;
        }

        private void Awake()
        {
            this.holdoutZoneController = base.GetComponent<HoldoutZoneController>();
            this.loadSiphonPrefabWaitHandle = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/moon2/BloodSiphonNearbyAttachment.prefab");
        }

        private void OnEnable()
        {
            // TODO: zone color (match blood pillar zone) this.holdoutZoneController.calcColor += this.ApplyColor;

            if (NetworkServer.active)
            {
                Transform transform/* = base.FindModelChild(this.siphonRootName);
                if (!transform)
                {
                    transform*/ = base.transform;

                //}
                this.siphonObject = UnityEngine.Object.Instantiate<GameObject>(this.loadSiphonPrefabWaitHandle.WaitForCompletion(), transform.position, transform.rotation, transform);
                NetworkServer.Spawn(this.siphonObject);
            }
        }

        private void OnDisable()
        {
            if (NetworkServer.active && this.siphonObject)
            {
                NetworkServer.Destroy(this.siphonObject);
            }

            // this.holdoutZoneController.calcColor -= this.ApplyColor;
        }
    }
}