using HG;
using RoR2;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.FinalStage
{
    public class FinalBossTest : Module
    {
        public override void Init()
        {
            On.RoR2.InfiniteTowerRun.Start += this.InfiniteTowerRun_Start;
        }

        private void InfiniteTowerRun_Start(On.RoR2.InfiniteTowerRun.orig_Start orig, InfiniteTowerRun self)
        {
            orig(self);
            self.EnsureComponent<FinalBossTestBehavior>();
        }

        public class FinalBossTestBehavior : MonoBehaviour
        {
            public void Update()
            {
                if (Input.GetKeyUp(KeyCode.F3))
                {
                    Console.CheatsConVar.instance.boolValue = true;
                    if (SceneCatalog.GetSceneDefForCurrentScene()?.cachedName == "meridian")
                    {
                        this.TeleportPlayerToBossArena();
                    }
                    else
                    {
                        this.SetStageToPrimeMeridian();
                    }
                }
            }

            private void SetStageToPrimeMeridian()
            {
                Stage.instance.BeginAdvanceStage(SceneCatalog.GetSceneDefFromSceneName("meridian"));
            }

            private void TeleportPlayerToBossArena()
            {
                var playerCollider = PlayerCharacterMasterController.instances.Where(x => x.isConnected && x.master).Select(x => x.master.GetBody()).Where(x => x).FirstOrDefault()?.GetComponent<Collider>();

                var lightningTrigger = FindObjectOfType<MeridianEventLightningTrigger>();

                if (lightningTrigger)
                {
                    lightningTrigger.OnTriggerExit(playerCollider);
                }

                MeridianEventTriggerInteraction.instance.OnTriggerEnter(playerCollider);
            }
        }
    }
}