using System.Windows;
using WPFApp.SaveResults;

namespace WPFApp.Rows
{
    public interface IRow
    {
        bool IsMovable { get; }

        bool IsRemovable { get; }

        bool IsSelected { get; set; }

        SaveResult TrySaveChanges();
    }
}