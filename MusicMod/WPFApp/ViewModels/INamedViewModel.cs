namespace WPFApp.ViewModels
{
    internal interface INamedViewModel
    {
        string Name { get; set; }

        string NameWatermark { get; }
    }
}