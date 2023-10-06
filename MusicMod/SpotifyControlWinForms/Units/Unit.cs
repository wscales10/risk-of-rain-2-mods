using Utils;

namespace SpotifyControlWinForms.Units
{
    public delegate bool PropertyChangingEventHandler<in T>(T newValue);

    public delegate void PropertyChangedEventHandler<in T>(object source, T newValue);

    public delegate TIn InputRequestedEventHandler<TIn, TOut>(Unit<TIn, TOut> unit);

    public abstract class Unit<TIn, TOut> : UnitBase
    {
        private TIn? cachedInput;

        protected Unit(string name) : base(name)
        {
        }

        public event Action<UnitUpdateInfo<TOut>>? Trigger;

        public void Ingest(TIn input)
        {
            this.Log($"[{DateTime.Now}] {Name}: {nameof(Ingest)}");
            var properties = typeof(TIn).GetProperties().Where(p => p.GetIndexParameters().Length == 0).ToList();
            var relevantProperties = properties.Where(p =>
            {
                return !Unit<TIn, TOut>.SpecialEquals(cachedInput is null ? null : p.GetValue(cachedInput), input is null ? null : p.GetValue(input), p.PropertyType);
            }).ToList();
            var changedPropertyNames = relevantProperties.Select(p => p.Name).ToList();

            cachedInput = input;

            if (IsEnabled)
            {
                HandleInput(input, changedPropertyNames);
            }
        }

        protected virtual void HandleInput(TIn input, IList<string> changedPropertyNames) => Output(Transform(input), changedPropertyNames);

        protected abstract TOut Transform(TIn input);

        protected void Output(TOut output, IList<string> changedPropertyNames)
        {
            this.Log($"[{DateTime.Now}] {Name}: {nameof(Output)}");
            Trigger?.Invoke(new(output, changedPropertyNames));
        }

        private static bool SpecialEquals(object? object1, object? object2, Type type)
        {
            if (type.IsGenericType(typeof(IList<>)))
            {
                var collection1 = (IEnumerable<object>?)object1;
                var collection2 = (IEnumerable<object>?)object2;

                if (collection1 is null)
                {
                    return collection2 is null || !collection2.Any();
                }
                else
                {
                    return collection2 is null ? !collection1.Any() : collection1.Zip(collection2, (a, b) => Equals(a, b)).All(x => x);
                }
            }
            else
            {
                return Equals(object1, object2);
            }
        }
    }
}