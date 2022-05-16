namespace Patterns.Patterns
{
    public interface IOnlyChildPattern<TChildPatternValue> : IPattern
    {
        IPattern<TChildPatternValue> Child { get; set; }
    }
}