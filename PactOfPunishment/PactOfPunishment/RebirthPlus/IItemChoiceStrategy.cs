namespace PactOfPunishment.RebirthPlus
{
    public interface IItemChoiceStrategy
    {
        PickupInfo? ChoosePickup(ILevelInfo levelInfo);
    }
}