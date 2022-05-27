namespace WPFApp.SaveResults
{
    public interface ISaveResult
    {
        bool IsSuccess { get; }

        bool? Status { get; }
    }

    public interface ISaveResult<out T> : ISaveResult
    {
        T Value { get; }
    }
}