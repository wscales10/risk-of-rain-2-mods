namespace Patterns.Patterns.SmallPatterns
{
	public interface IValuePattern : ISmallPattern
	{
        bool Redefinable { get; }

        IValuePattern DefineWith(string stringDefinition);
	}
}
