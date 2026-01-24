using AssortedExperiments.BugFixes;
using BepInEx;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace AssortedExperiments
{
    [BepInPlugin("com.woodyscales.assortedexperiments", "Assorted Experiments", "1.0.0")]
    public class AssortedExperiments : BaseUnityPlugin
    {
        private Module[]? modules;

        public void Awake()
        {
            var settings = new Settings(this.Config);

            this.modules = new Module[]
            {
                new GildedEliteBugfix(),
                new Diagnostics.Diagnostics(),
                new ItemAdjustments.EccentricVase(),
                new ItemAdjustments.RegenerativeScrap(),
                new ItemAdjustments.Transcendence(),
                new ItemAdjustments.WarBonds(),
                new ItemBias.ItemBias(),
                new Items.AssortedItems(),
                new StageFeatures.StageFeatures()
            };

            foreach (var module in this.modules)
            {
                module.Logger = this.Logger;
                module.Settings = settings;
                module.Init();
            }
        }
    }
}