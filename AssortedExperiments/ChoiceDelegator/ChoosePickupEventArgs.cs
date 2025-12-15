using RoR2;

namespace ChoiceDelegator
{
    public class ChoosePickupEventArgs : RoutedEventArgs
    {
        public ChoosePickupEventArgs(GenericPickupController.CreatePickupInfo pickupInfo)
        {
            this.Pickup = pickupInfo.pickup;
        }

        public UniquePickup Pickup { get; set; }
    }
}