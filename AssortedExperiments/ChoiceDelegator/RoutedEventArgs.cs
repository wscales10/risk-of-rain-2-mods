using System;

namespace ChoiceDelegator
{
    public class RoutedEventArgs : EventArgs
    {
        public bool Handled { get; set; }

        public static void RaiseEvent<TArgs>(object sender, TArgs e, EventHandler<TArgs>? eventHandler)
            where TArgs : RoutedEventArgs
        {
            Delegate[] handlers = eventHandler?.GetInvocationList() ?? Array.Empty<Delegate>();

            foreach (var handler in handlers)
            {
                if (e.Handled)
                {
                    break;
                }

                if (handler is EventHandler<TArgs> typedHandler)
                {
                    typedHandler(sender, e);
                }
            }
        }
    }
}