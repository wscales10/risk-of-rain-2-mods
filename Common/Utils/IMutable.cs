namespace Utils
{
    public interface IMutable<out TInterface>
    {
        TInterface ToReadOnly();
    }
}