using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using Utils.Reflection.Properties;

namespace WPFApp.SaveResults
{
    public class SaveResult : ISaveResult
    {
        public SaveResult(bool? status, params Action[] release) : this(status, new Queue<Action>(release))
        {
        }

        public SaveResult(bool? status, IEnumerable<Action> release)
        {
            Status = status;
            ReleaseActions = IsSuccess ? new Queue<Action>(release) : new Queue<Action>();
        }

        private SaveResult(bool? status, Queue<Action> release1, Queue<Action> release2) : this(status, release1.Concat(release2))
        {
        }

        public Queue<Action> ReleaseActions { get; }

        public bool IsSuccess => Status ?? true;

        public bool? Status { get; }

        public static SaveResult operator &(SaveResult result1, ISaveResult result2) => new(And(result1.Status, result2.Status), result1.ReleaseActions);

        public static SaveResult operator &(SaveResult result1, SaveResult result2) => new(And(result1.Status, result2.Status), result1.ReleaseActions, result2.ReleaseActions);

        public static SaveResult operator &(bool b, SaveResult result) => new(And(b, result.Status), result.ReleaseActions);

        public static bool operator true(SaveResult result) => result.IsSuccess;

        public static bool operator false(SaveResult result) => !result.IsSuccess;

        public static SaveResult<T> From<T>(T value) => new(value);

        public static SaveResult<T> Create<T>(SaveResult result)
        {
            switch (result)
            {
                case SaveResult<T> srt:
                    return srt;

                case ISaveResult<T> isrt:
                    return new(result, isrt.Value);

                case null:
                    return null;

                default:
                    if (result.GetType().GetInterfaces().Any(i => i.IsGenericType(typeof(ISaveResult<>)) && i.GenericTypeArguments[0].IsAssignableFrom(typeof(T))))
                    {
                        return new(result, result.GetPropertyValue<T>(nameof(ISaveResult<T>.Value)));
                    }

                    throw new ArgumentOutOfRangeException(nameof(result));
            }
        }

        public void Release()
        {
            while (ReleaseActions.Count > 0)
            {
                ReleaseActions.Dequeue()();
            }
        }

        protected static bool? And(bool? b1, bool? b2)
        {
            if (b1 == false || b2 == false)
            {
                return false;
            }

            return b1 ?? b2;
        }
    }

    public class SaveResult<T> : SaveResult, ISaveResult<T>
    {
        public SaveResult(T value) : base(true) => Value = value;

        public SaveResult(SaveResult result, T value) : this(result.Status, result.ReleaseActions, value)
        {
        }

        public SaveResult(bool? status, T value = default) : base(status) => Value = value;

        private SaveResult(bool? status, Queue<Action> releaseActions, T value) : base(status, releaseActions) => Value = value;

        public T Value { get; }

        public static SaveResult<T> operator &(SaveResult<T> result, bool b) => new(And(result.Status, b), result.ReleaseActions, result.Value);

        public static SaveResult<T> operator &(SaveResult<T> result1, ISaveResult result2) => new((SaveResult)result1 & result2, result1.Value);

        public static SaveResult<T> operator &(SaveResult<T> result1, SaveResult result2) => new((SaveResult)result1 & result2, result1.Value);
    }
}