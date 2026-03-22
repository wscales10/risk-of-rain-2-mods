using HG;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite3
{
    public class Halcyonite3 : Stage1HalcyoniteBossWaveDefinition
    {
        private readonly CharacterSpawnCard customSpawnCard;

        public Halcyonite3()
        {
            CharacterSpawnCard orig = base.GetHalcyoniteSpawnCard();
            CharacterSpawnCard spawnCardCopy = Object.Instantiate(orig);
            GameObject dustCenterPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ImpBoss/ImpBossBody.prefab").WaitForCompletion().GetComponent<ModelLocator>().modelTransform.GetComponent<ChildLocator>().FindChild("DustCenter").gameObject;
            GameObject dustCenter = dustCenterPrefab.InstantiateClone(dustCenterPrefab.name);
            spawnCardCopy.prefab = spawnCardCopy.prefab.InstantiateClone("Halcyonite3Master");
            var modelTransform = spawnCardCopy.prefab.GetComponent<CharacterMaster>().bodyPrefab.GetComponent<ModelLocator>().modelTransform;
            dustCenter.transform.SetParent(modelTransform, false);
            modelTransform.GetComponent<ChildLocator>().AddChild(dustCenter.name, dustCenter.transform);
            this.customSpawnCard = spawnCardCopy;
        }

        protected override void Setup(CombatDirector dir, CombatSquad squad, InfiniteTowerExplicitSpawnWaveController wavePrefab)
        {
            base.Setup(dir, squad, wavePrefab);
            wavePrefab.EnsureComponent<Halcyonite3BossFightBehavior>();
        }

        protected override CharacterSpawnCard GetHalcyoniteSpawnCard() => this.customSpawnCard;
    }
}