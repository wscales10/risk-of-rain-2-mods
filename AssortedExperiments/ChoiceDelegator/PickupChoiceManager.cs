using System;

namespace ChoiceDelegator
{
    public static class PickupChoiceManager
    {
        public static event EventHandler<ChoosePickupEventArgs>? ChoosePickup;

        public static void OnChoosePickup(object sender, ChoosePickupEventArgs e)
        {
            RoutedEventArgs.RaiseEvent(sender, e, ChoosePickup);
        }
    }
}