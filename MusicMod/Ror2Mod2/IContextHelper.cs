using System;

namespace Ror2Mod2
{
    public interface IContextHelper<out TContext>
    {
        event Action<TContext> NewContext;
    }
}