using System.Windows;
using WPFApp.Controls.Wrappers.SaveResults;

namespace WPFApp.Controls.Rows
{
    public interface IRow
    {
        UIElement LeftElement { get; }

        bool IsMovable { get; }

        bool IsRemovable { get; }

        bool IsSelected { get; set; }

        UIElement RightElement { get; }

        SaveResult TrySaveChanges();
    }
}