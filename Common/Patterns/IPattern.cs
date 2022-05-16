using Utils;

namespace Patterns
{
    public interface IAndSimplifiable<T>
    {
        IPattern<T> SimplifyAnd(IPattern<T> other);
    }

    public interface INotSimplifiable<T>
    {
        IPattern<T> SimplifyNot();
    }

    public interface IOrSimplifiable<T>
    {
        IPattern<T> SimplifyOr(IPattern<T> other);
    }

    public interface IPattern : IXmlExportable
    {
        IPattern Correct();

        bool IsMatch(object obj);
    }

    public interface IPattern<in T> : IPattern
    {
        bool IsMatch(T value);

        IPattern<T> Simplify();
    }
}