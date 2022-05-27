using System.ComponentModel;

namespace WPFApp.ViewModels.Commands
{
    internal interface IPropertyString : INotifyDataErrorInfo, INotifyPropertyChanged
    {
        string AsString { get; }

        bool IsRequired { get; }

        string Prefix { get; }

        string PropertyName { get; }

        string Suffix { get; }

        object ValueObject { get; set; }
    }
}