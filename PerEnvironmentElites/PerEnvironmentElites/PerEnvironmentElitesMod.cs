using BepInEx;
using RoR2;
using System;
using System.Linq;

namespace PerEnvironmentElites
{
    [BepInPlugin("com.woodyscales.perenvironmentelites", "Per-Environment Elites", "1.0.0")]
    public class PerEnvironmentElitesMod : BaseUnityPlugin
    {
        private readonly IWeightGetter weightGetter;
        private readonly IEnvironmentProvider environmentProvider;

        public PerEnvironmentElitesMod()
        {
            this.weightGetter = new CsvWeightGetter(this.Logger);
            this.environmentProvider = new SceneCatalogEnvironmentProvider();
        }

        public void Awake()
        {
            this.Try(this.weightGetter.Init);

            On.RoR2.CombatDirector.Init += this.CombatDirector_Init;

            On.RoR2.CombatDirector.EliteTierDef.GetRandomAvailableEliteDef += this.EliteTierDef_GetRandomAvailableEliteDef;

            On.RoR2.SceneCatalog.OnActiveSceneChanged += this.SceneCatalog_OnActiveSceneChanged;

            On.RoR2.PauseStopController.Pause += this.PauseStopController_Pause;
        }

        private void PauseStopController_Pause(On.RoR2.PauseStopController.orig_Pause orig, PauseStopController self, bool shouldPause)
        {
            if (!shouldPause)
            {
                this.Try(this.weightGetter.Init);
            }

            orig(self, shouldPause);
        }

        private void SceneCatalog_OnActiveSceneChanged(On.RoR2.SceneCatalog.orig_OnActiveSceneChanged orig, UnityEngine.SceneManagement.Scene oldScene, UnityEngine.SceneManagement.Scene newScene)
        {
            this.Logger.LogDebug($"Switching from scene '{oldScene.name}' to scene '{newScene.name}'...");
            switch (SceneCatalog.GetSceneDefFromScene(newScene).sceneType)
            {
                case SceneType.Stage:
                case SceneType.Intermission:
                case SceneType.TimedIntermission:
                case SceneType.UntimedStage:
                    this.Try(this.weightGetter.Init);
                    break;

                default:
                    break;
            }

            orig(oldScene, newScene);
            this.Logger.LogDebug($"Switched from scene '{oldScene.name}' to scene '{newScene.name}'.");
        }

        private void CombatDirector_Init(On.RoR2.CombatDirector.orig_Init orig)
        {
            orig();

            for (int i = 0; i < CombatDirector.eliteTiers.Length; i++)
            {
                this.Logger.LogDebug($"Elite tier {i} contains:");

                foreach (var elite in CombatDirector.eliteTiers[i].eliteTypes)
                {
                    this.Logger.LogDebug($"> '{elite?.name}'");
                }
            }
        }

        private EliteDef? EliteTierDef_GetRandomAvailableEliteDef(On.RoR2.CombatDirector.EliteTierDef.orig_GetRandomAvailableEliteDef orig, CombatDirector.EliteTierDef self, Xoroshiro128Plus rng)
        {
            WeightedSelection<EliteDef> weightedSelection = new WeightedSelection<EliteDef>(self.eliteTypes.Length);

            self.availableDefs.Clear();
            SceneDef currentEnvironment = this.environmentProvider.GetCurrentEnvironment();

            foreach (var eliteDef in self.eliteTypes.Where(eliteDef => eliteDef && eliteDef.IsAvailable()))
            {
                decimal weight = this.weightGetter.GetWeight(eliteDef, currentEnvironment);

                if (weight > 0)
                {
                    self.availableDefs.Add(eliteDef);
                    weightedSelection.AddChoice(eliteDef, (float)weight);
                }
            }

            if (self.availableDefs.Count > 0)
            {
                return weightedSelection.Evaluate(rng.nextNormalizedFloat);
            }

            return null;
        }

        private void Try(Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex);
            }
        }
    }
}