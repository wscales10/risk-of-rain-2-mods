namespace WPFApp.ViewModels
{
    public interface IValidationRule
    {
        bool Validate(object obj);

        string GetErrorMessage(object obj);
    }
}