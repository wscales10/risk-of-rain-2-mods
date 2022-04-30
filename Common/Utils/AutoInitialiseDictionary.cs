namespace Utils
{
	public class AutoInitialiseDictionary<TKey, TValue> : Cache<TKey, TValue>
		where TValue : new()
	{
		public AutoInitialiseDictionary() : base(_ => new TValue())
		{
		}
	}
}