namespace WPFApp.Controls.Wrappers.SaveResults
{
    public interface ISaveResult
    {
        bool IsSuccess { get; }

        bool? Status { get; }
    }

    public interface ISaveResult<T> : ISaveResult
    {
        T Value { get; }
    }
}