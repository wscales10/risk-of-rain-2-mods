namespace Patterns.Patterns.SmallPatterns
{
	public interface IValuePattern : ISmallPattern
	{
		IValuePattern DefineWith(string stringDefinition);
	}
}
