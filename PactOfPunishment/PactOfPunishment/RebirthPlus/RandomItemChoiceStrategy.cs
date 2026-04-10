using RoR2;
using System.Linq;

namespace PactOfPunishment.RebirthPlus
{
    public class RandomItemChoiceStrategy : IItemChoiceStrategy
    {
        public PickupInfo? ChoosePickup(ILevelInfo levelInfo)
        {
            PickupInfo[] options = levelInfo.Options.Where(x => x.IsAvailable).ToArray();

            if (options.Length == 0)
            {
                return null;
            }

            return Run.instance.treasureRng.NextElementUniform(options);
        }
    }
}