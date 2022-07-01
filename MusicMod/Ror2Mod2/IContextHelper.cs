using System;

namespace Ror2Mod2
{
    public interface IContextHelper<TContext>
    {
        event Action<TContext> NewContext;
    }
}