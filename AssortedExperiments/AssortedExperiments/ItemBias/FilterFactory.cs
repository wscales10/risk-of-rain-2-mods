using BepInEx.Logging;
using RoR2;

namespace AssortedExperiments.ItemBias
{
    public class FilterFactory
    {
        private readonly ManualLogSource logger;

        private readonly Settings settings;

        private TestFilter? testFilter;

        private AllPlayersFilter? allPlayersFilter;

        public FilterFactory(ManualLogSource logger, Settings settings)
        {
            this.logger = logger;
            this.settings = settings;
        }

        public IFilter GetFilter(PlayerCharacterMasterController? player)
        {
            if (this.settings.TestMode)
            {
                return this.testFilter ??= new TestFilter(this.logger);
            }

            if (player)
            {
                return new PlayerFilter(player!.master, this.logger, this.settings);
            }

            return this.allPlayersFilter ??= new AllPlayersFilter(this.logger, this.settings);
        }
    }
}