namespace WPFApp
{
	internal interface IChange<out T> where T : IChange<T>
	{
		T Reversed { get; }
	}
}