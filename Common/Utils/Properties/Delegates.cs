namespace Utils.Properties
{
	public delegate void PropertySetEventHandler<T>(T oldValue, T newValue);

	public delegate void PropertySetEventHandler<T1, T2>(T1 oldValue, T1 newValue, T2 extraParam);

	public delegate bool PropertyTrySetEventHandler<T>(T oldValue, T newValue);

	public delegate bool PropertyTrySetEventHandler<T1, T2>(T1 oldValue, T1 newValue, T2 extraParam);

	public delegate void PropertyChangedEventHandler<T>(T newValue);

	public delegate void PropertyChangedEventHandler<T1, T2>(T1 newValue, T2 extraParam);
}