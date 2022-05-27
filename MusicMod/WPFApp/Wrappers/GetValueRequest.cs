namespace WPFApp.Wrappers
{
    public class GetValueRequest
    {
        public GetValueRequest(bool trySave, bool bypassValidation = false)
        {
            TrySave = trySave;
            BypassValidation = bypassValidation;
        }

        public bool TrySave { get; }

        public bool BypassValidation { get; }

        public static implicit operator GetValueRequest(bool b) => new(b);
    }
}