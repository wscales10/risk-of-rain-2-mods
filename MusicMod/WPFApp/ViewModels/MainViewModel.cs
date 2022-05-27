using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Win32;
using System.Windows;
using WPFApp.Rows;
using System.ComponentModel;
using System.IO;

namespace WPFApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private NavigationViewModelBase itemViewModel;

        private bool hasContent;

        private ICollectionView mainRows;

        private string title;

        public MainViewModel(NavigationContext navigationContext)
        {
            NavigationContext = navigationContext;
            NavigateTreeCommand = new(NavigateTree);
            BackCommand = new(_ => OnGoBack?.Invoke(), false);
            ForwardCommand = new(_ => OnGoForward?.Invoke(), false);
            ImportCommand = new(_ =>
            {
                OpenFileDialog dialog = new() { Filter = "XML Files (*.xml)|*.xml|All files (*.*)|*.*" };

                if (dialog.ShowDialog() == true)
                {
                    OnImportFile?.Invoke(dialog.FileName);
                }
            });
            ExportCommand = new(_ =>
            {
                if (TryGetExportLocation(out string fileName))
                {
                    OnExportFile?.Invoke(fileName);
                }
            });
            NewCommand = new(_ =>
            {
                if (MessageBox.Show("Are you sure you want to close this?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    OnReset?.Invoke();
                }
            }, this, nameof(HasContent));
            HomeCommand = new(_ => NavigationContext.GoHome(), NavigationContext, nameof(NavigationContext.IsHome), b => !(bool)b);
            UpCommand = new(_ => NavigationContext.GoUp(), NavigationContext, nameof(NavigationContext.IsHome), b => !(bool)b);
            ClearCacheCommand = new(_ => ClearCache());
            GotoPlaylistsCommand = new(_ => NavigationContext.GoInto(Info.Playlists));
        }

        public event Action OnReset;

        public event Action OnGoBack;

        public event Action OnGoForward;

        public event Action<string> OnImportFile;

        public event Action<string> OnExportFile;

        public ButtonCommand NavigateTreeCommand { get; }

        public ButtonCommand BackCommand { get; }

        public ButtonCommand ForwardCommand { get; }

        public ButtonCommand UpCommand { get; }

        public ButtonCommand HomeCommand { get; }

        public ButtonCommand ImportCommand { get; }

        public ButtonCommand ExportCommand { get; }

        public ButtonCommand NewCommand { get; }

        public ButtonCommand ClearCacheCommand { get; }

        public ButtonCommand GotoPlaylistsCommand { get; }

        public NavigationContext NavigationContext { get; }

        public NavigationViewModelBase ItemViewModel
        {
            get => itemViewModel;

            set
            {
                itemViewModel = value;
                HasContent = value is not null;
                ExportCommand.CanExecute = value is IXmlViewModel;
                NotifyPropertyChanged();
            }
        }

        public bool HasContent
        {
            get => hasContent;

            set => SetProperty(ref hasContent, value);
        }

        public ICollectionView MainRows
        {
            get => mainRows;

            set => SetProperty(ref mainRows, value);
        }

        public string Title
        {
            get => "Rule Builder" + (title is null ? string.Empty : $" ({title})");

            set => SetProperty(ref title, value);
        }

        private static void ClearCache() => Images.ClearCache();

        private static bool TryGetExportLocation(out string fileName)
        {
            SaveFileDialog dialog = new() { Filter = "XML Files (*.xml)|*.xml|All files (*.*)|*.*" };

            if (dialog.ShowDialog() == true)
            {
                fileName = dialog.FileName;
                return true;
            }
            else
            {
                fileName = null;
                return false;
            }
        }

        private void NavigateTree(object rowObject) => NavigationContext.NavigateTreeTo((IRuleRow)rowObject);
    }
}