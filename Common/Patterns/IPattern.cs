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
#error instead of using this when exporting, we should export enum patterns with Enum`1 and only convert to nongeneric EnumPattern if there's a problem during import

		IPattern Correct();

		bool IsMatch(object value);
	}

	public interface IPattern<in T> : IPattern
	{
		bool IsMatch(T value);

		IPattern<T> Simplify();
	}
}