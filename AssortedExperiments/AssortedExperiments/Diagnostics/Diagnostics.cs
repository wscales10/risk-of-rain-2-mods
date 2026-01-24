using RoR2;
using UnityEngine.SceneManagement;

namespace AssortedExperiments.Diagnostics
{
    public class Diagnostics : Module
    {
        public override void Init()
        {
            On.RoR2.SceneCatalog.OnActiveSceneChanged += this.SceneCatalog_OnActiveSceneChanged;
            On.RoR2.TeleporterInteraction.ChargedState.OnEnter += this.ChargedState_OnEnter;
        }

        private void ChargedState_OnEnter(On.RoR2.TeleporterInteraction.ChargedState.orig_OnEnter orig, TeleporterInteraction.ChargedState self)
        {
            orig(self);
            this.Logger.LogInfo($"Teleporter charged - {Utils.GetTimeString()}");
        }

        private void SceneCatalog_OnActiveSceneChanged(On.RoR2.SceneCatalog.orig_OnActiveSceneChanged orig, Scene oldScene, Scene newScene)
        {
            orig(oldScene, newScene);
            this.Logger.LogInfo($"Scene changed from '{Utils.GetSceneDisplayName(oldScene)}' to '{Utils.GetSceneDisplayName(newScene)}'- {Utils.GetTimeString()}");
        }
    }
}