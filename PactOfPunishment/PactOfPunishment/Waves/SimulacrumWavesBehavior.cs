using R2API;
using RoR2;
using System;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves
{
    public partial class SimulacrumWavesBehavior : MonoBehaviour
    {
        private InfiniteTowerRun run;

        private GameObject? defaultMithrix;

        private GameObject? runaldAndKjaro;

        private GameObject? solusControlUnit;

        private GameObject? impOverlord;

        private GameObject? wormAndDistributor;

        private GameObject? projectilers;

        private GameObject? summoner;

        public bool TryOverrideWeightedSelection(InfiniteTowerWaveCategory self)
        {
            try
            {
                self.weightedSelection.Clear();
                self.weightedSelection.AddChoice(this.wormAndDistributor, 1);

                if (self.name != "BossWaveCategory")
                {
                    return false;
                }

                if (this.run.waveIndex == 5)
                {
                    self.weightedSelection.Clear();
                    self.weightedSelection.AddChoicesWithRelativeWeight(1, x => x, (this.runaldAndKjaro, 1), (this.solusControlUnit, 1), (this.impOverlord, 1));
                    return true; // TODO: make this method more robust so there are no problems if you forget to return true
                }

                if (this.run.waveIndex == 15)
                {
                    self.weightedSelection.RemoveWhere(x => x.GetComponent<InfiniteTowerWaveController>() is InfiniteTowerExplicitSpawnWaveController);
                    self.weightedSelection.AddChoicesWithRelativeWeight(1, x => x, (this.wormAndDistributor, 1), (this.projectilers, 1));
                    return true;
                }

                if (this.run.waveIndex == 20)
                {
                    self.weightedSelection.Clear();
                    self.weightedSelection.AddChoice(this.summoner, 1);
                    return true;
                }

                if (this.run.waveIndex == 25)
                {
                    self.weightedSelection.Clear();
                    self.weightedSelection.AddChoice(this.defaultMithrix, 0.5f);

                    // self.weightedSelection.AddChoice(this.beetleQueen, 0.5f);
                    return true;
                }

                self.weightedSelection.RemoveWhere(x => x == this.defaultMithrix);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return false;
        }

        private void Awake()
        {
            this.run = this.GetComponent<InfiniteTowerRun>();

            // Stage 1
            this.runaldAndKjaro = new RunaldAndKjaro().MakeWavePrefab(this.run);
            this.solusControlUnit = new SolusControlUnit().MakeWavePrefab(this.run);
            this.impOverlord = new ImpOverlord().MakeWavePrefab(this.run);

            // Stage 2
            this.wormAndDistributor = new WormAndDistributor().MakeWavePrefab(this.run);
            this.projectilers = new Projectilers().MakeWavePrefab(this.run);

            this.summoner = new Summoner().MakeWavePrefab(this.run);

            // Stage 3
            this.defaultMithrix = this.run.waveCategories.Single(x => x.name == "BossWaveCategory").wavePrefabs.Select(x => x.wavePrefab).Single(x => x.name == "InfiniteTowerWaveBossBrother");
        }
    }
}