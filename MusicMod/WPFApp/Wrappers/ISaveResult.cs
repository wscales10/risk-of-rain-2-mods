using System;
using System.Collections.Generic;

namespace WPFApp.Wrappers
{
    public interface ISaveResult
    {
        bool IsSuccess { get; }

        bool? Status { get; }

        Queue<Action> ReleaseActions { get; }
    }

    public interface ISaveResult<out T> : ISaveResult
    {
        T Value { get; }
    }
}