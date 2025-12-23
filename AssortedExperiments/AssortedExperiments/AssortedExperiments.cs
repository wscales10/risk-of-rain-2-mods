using BepInEx;
using RoR2;
using System.Collections.Generic;

namespace AssortedExperiments
{
    [BepInPlugin("com.woodyscales.assortedexperiments", "Assorted Experiments", "1.0.0")]
    public class AssortedExperiments : BaseUnityPlugin
    {
        private AwokenMod? mod;

        public void Awake()
        {
            var settings = new Settings(this.Config);
            var waitingForScrapper = new HashSet<SceneDirector>();
            var on = new OnHooks(this.Logger, settings, waitingForScrapper);
            var il = new ILHooks(this.Logger, settings, waitingForScrapper);
            this.mod = new AwokenMod(on, il);
        }
    }
}