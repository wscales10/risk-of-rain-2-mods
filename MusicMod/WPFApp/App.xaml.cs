using HtmlAgilityPack;
using Patterns;
using Patterns.Patterns;
using Rules.RuleTypes.Mutable;
using Spotify;
using Spotify.Authorisation;
using Spotify.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using Utils;
using WPFApp.Controls;
using WPFApp.Controls.Rows;
using WPFApp.Controls.Wrappers.PatternWrappers;
using WPFApp.Properties;
using WPFApp.Views;
using System.Diagnostics.CodeAnalysis;
using WPFApp.ViewModels;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Threading;

namespace WPFApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly History<Navigation> history = new();

        private readonly MutableNavigationContext navigationContext = new();

        private readonly Cache<object, NavigationViewModelBase> viewModels;

        private readonly Timer timer;

        private Action display;

        private MainViewModel mainViewModel;

        public App()
        {
            timer = new(_ => PlaybackClient.Stop());
            exportTaskMachine = new(exportCancellationTokenSource.Token);

            // TODO: (not sure where to put this) Would be nice to have options to define "playlists" in app rather than in spotify:
            // - Play track
            // - Wait length of track (+delay?) (abort if bucket switches)
            // - Play next track, etc.
            viewModels = new(item => item switch
            {
                Rule rule => GetRuleViewModel(rule),
                IPattern pattern => GetPatternControl(pattern),
                _ => null,
            });

            navigationContext.OnGoHome += GoHome;
            navigationContext.OnGoUp += GoUp;
            navigationContext.OnGoInto += Display;
            navigationContext.ViewModelRequested += (obj) => obj is null ? null : viewModels[obj];
            NavigationContext = new(navigationContext);
            history.ActionRequested += TryInner;
            MetadataClient = new SpotifyMetadataClient(x => MetadataClient.Log(x));
            MetadataClient.OnError += SpotifyClient_OnError;
            PlaybackClient = new SpotifyPlaybackClient(x => PlaybackClient.Log(x));
            PlaybackClient.OnError += SpotifyClient_OnError;
            Authorisation = new Authorisation(Scopes.Metadata.Concat(Scopes.Playback), logger: x => Authorisation.Log(x));
            Settings.Default.PropertyChanged += OnSettingChanged;
        }

        private event Func<FileInfo> OnAutosaveLocationRequested;

        public static FileInfo EffectiveAutosaveLocation => Settings.Default.Autosave ? AutosaveLocation : null;

        public static FileInfo AutosaveLocation
        {
            get => string.IsNullOrEmpty(Settings.Default.AutosaveLocation) ? null : new(Settings.Default.AutosaveLocation);

            set => Settings.Default.AutosaveLocation = value?.FullName;
        }

        public NavigationContext NavigationContext { get; }

        public Authorisation Authorisation { get; }

        public SpotifyMetadataClient MetadataClient { get; }

        public SpotifyPlaybackClient PlaybackClient { get; }

        public List<NavigationViewModelBase> ViewModelList { get; set; } = new();

        public NavigationViewModelBase CurrentViewModel => ViewModelList.LastOrDefault();

        public void GoBack()
        {
            _ = history.Undo();
            display();
        }

        public void GoForward()
        {
            _ = history.Redo();
            display();
        }

        public void GoHome()
        {
            _ = Try(new RemoveNavigation(ViewModelList.Skip(1).Reverse()));
            display();
        }

        public NavigationViewModelBase Display(IEnumerable list)
        {
            NavigationViewModelBase output = null;
            foreach (object item in list)
            {
                NavigationViewModelBase viewModel = (item as NavigationViewModelBase) ?? (item is null ? null : viewModels[item]);

                if (Try(new AddNavigation(viewModel)))
                {
                    display();
                    output = CurrentViewModel;
                }
                else
                {
                    display();
                    break;
                }
            }

            return output;
        }

        public bool GoUp(int count)
        {
            var result = Try(new RemoveNavigation(ViewModelList.Reverse<NavigationViewModelBase>().Take(count)));
            display();
            return result;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainView mainView = new(NavigationContext);
            Attach(mainView);
            RegisterRequestHandlers();

            if (EffectiveAutosaveLocation is not null)
            {
                try
                {
                    ImportFile(EffectiveAutosaveLocation.FullName);
                }
                catch (XmlException)
                {
                    Settings.Default.Autosave = false;
                }
            }
            else
            {
                ImportXml(Rules.Examples.MimicRule.ToXml());
            }

            display();
            mainView.Show();

            //new ControlTestView().Show();

            Authorisation.OnAccessTokenReceived += (_, t) =>
            {
                MetadataClient.GiftNewAccessToken(t);
                PlaybackClient.GiftNewAccessToken(t);
                SpotifyItemPickerViewModel.Refresh();
            };
            Authorisation.OnClientRequested += Web.Goto;
            if (!Settings.Default.OfflineMode)
            {
                Authorisation.InitiateScopeRequest();
            }
        }

        private void RegisterRequestHandlers()
        {
            SpotifyItemPicker.MusicItemInfoRequested += HandleMusicItemInfoRequestAsync;
            PatternWrapper.OnHtmlWebRequested += HandleHtmlWebRequest;
            BucketRow.OnCommandPreviewRequested += HandleCommandPreviewRequestAsync;
        }

        private void Attach(MainView mainView)
        {
            mainViewModel = (MainViewModel)mainView.DataContext;
            mainViewModel.OnGoBack += GoBack;
            mainViewModel.OnGoForward += GoForward;
            mainViewModel.OnImportFile += ImportFile;
            mainViewModel.OnExportFile += ExportToFile;
            mainViewModel.OnReset += () => Reset();
            mainView.OnTryEnableAutosave += TryEnableAutosave;
            mainView.OnTryClose += TryClose;
            OnAutosaveLocationRequested += MainView.GetExportLocation;

            display = () =>
            {
                navigationContext.IsHome = ViewModelList.Count < 2;
                mainViewModel.BackCommand.CanExecute = history.CurrentIndex > 0;
                mainViewModel.ForwardCommand.CanExecute = history.ReverseIndex > 0;
                mainViewModel.ItemViewModel = CurrentViewModel;
            };
        }

        [SuppressMessage("Usage", "VSTHRD001:Avoid legacy thread switching APIs", Justification = "It works, the new one doesn't")]
        private bool TryClose()
        {
            MaybeSave();

            exportTaskMachine.Close();

            /*if (!exportTaskMachine.Lifecycle.IsCompleted)
            {
                ExportWindow exportWindow = new();
                _ = exportTaskMachine.Lifecycle.ContinueWith(_ => Dispatcher.Invoke(() =>
                {
                    exportWindow.Close();
                    Shutdown();
                }), TaskScheduler.Default);
                exportWindow.Show();
                return false;
            }*/

            return true;
        }

        private bool TryEnableAutosave()
        {
            if (TrySave())
            {
                Settings.Default.Autosave = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnSettingChanged(object sender, PropertyChangedEventArgs e)
        {
            var settings = (Settings)sender;
            settings.Save();

            switch (e.PropertyName)
            {
                case nameof(Settings.OfflineMode):

                    // TODO: Authorisation needs improving so it stops if it fails to connect and it can be restarted at any time. There should be a public function to manually request a refreshed token or restart the process.
                    if (settings.OfflineMode)
                    {
                        _ = Authorisation.PauseAsync();
                    }
                    else if (Authorisation.Lifecycle.IsRunning)
                    {
                        Authorisation.Resume();
                        SpotifyItemPickerViewModel.Refresh();
                    }
                    else
                    {
                        Authorisation.InitiateScopeRequest();
                    }

                    break;

                case nameof(Settings.AutosaveLocation):
                case nameof(Settings.Autosave):
                    mainViewModel.Title = EffectiveAutosaveLocation?.Name;
                    break;
            }
        }

        private void SpotifyClient_OnError(Exception e) => _ = MessageBox.Show(
            e.Message,
            Utils.HelperMethods.AddSpacesToPascalCaseString(e.GetType().Name) + " (Spotify Client)",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        private NavigationViewModelBase GetPatternControl(IPattern pattern)
        {
            if (pattern is IListPattern lp)
            {
                return new ListPatternViewModel(lp, lp.ValueType, NavigationContext);
            }

            return null;
        }

        private void Reset(NavigationViewModelBase viewModel = null, bool resetAutosave = true)
        {
            if (resetAutosave)
            {
                AutosaveLocation = null;
            }

            ViewModelList.Clear();

            if (viewModel is not null)
            {
                ViewModelList.Add(viewModel);
                SetMainRows();
            }
            else
            {
                mainViewModel.MainRows = null;
            }

            history.Clear();
            display();
        }

        private NavigationViewModelBase GetRuleViewModel(Rule rule) => rule switch
        {
            StaticSwitchRule sr => new SwitchRuleViewModel(sr, NavigationContext),
            ArrayRule ar => new ArrayRuleViewModel(ar, NavigationContext),
            IfRule ir => new IfRuleViewModel(ir, NavigationContext),
            Bucket b => new BucketViewModel(b, NavigationContext),
            _ => null,
        };

        private async Task<ConditionalValue<MusicItemInfo>> HandleMusicItemInfoRequestAsync(SpotifyItem item)
        {
            if (Settings.Default.OfflineMode || !MetadataClient.IsAuthorised)
            {
                return new();
            }

            MusicItemInfo info = null;

            GetMusicItemInfoCommand command = new(item, i =>
            {
                info = i;
                return Task.CompletedTask;
            });

            await MetadataClient.Do(command);

            if (command.Task is not null)
            {
                await command.Task;
            }

            return new(info);
        }

        private HtmlWeb HandleHtmlWebRequest() => Settings.Default.OfflineMode ? null : new HtmlWeb();

        private async Task HandleCommandPreviewRequestAsync(Command command)
        {
            if (Settings.Default.OfflineMode)
            {
                return;
            }

            await PlaybackClient.Do(command);
            timer.Change(TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
        }

        private void PingSpotify()
        {
            Ping ping = new();
            var pingReply = ping.Send("accounts.spotify.com");
            Settings.Default.OfflineMode = pingReply.Status != IPStatus.Success;
        }

        private bool Try(Navigation navigation) => history.Try(navigation, CurrentViewModel is not null);

        private void MaybeSave(bool isViewModelSaved = false)
        {
            if (Settings.Default.Autosave && !TrySave(isViewModelSaved))
            {
                Settings.Default.Autosave = false;
            }
        }

        private bool TrySave(bool isViewModelSaved = false)
        {
            if (ViewModelList[0] is not IXmlViewModel xmlViewModel)
            {
                return false;
            }

            if (!isViewModelSaved && !CurrentViewModel.TrySave().IsSuccess)
            {
                return false;
            }

            if ((AutosaveLocation ??= OnAutosaveLocationRequested?.Invoke()) is null)
            {
                return false;
            }

            ExportToFile(xmlViewModel, AutosaveLocation.FullName);
            return true;
        }

        private bool TryInner(Navigation navigation)
        {
            switch (navigation)
            {
                case AddNavigation:
                    foreach (NavigationViewModelBase viewModel in navigation.ViewModels)
                    {
                        if (!TrySaveViewModel())
                        {
                            return false;
                        }

                        MaybeSave(true);
                        ViewModelList.Add(viewModel);
                        if (ViewModelList.Count == 1)
                        {
                            SetMainRows();
                        }
                    }
                    break;

                case RemoveNavigation:
                    foreach (NavigationViewModelBase viewModel in navigation.ViewModels)
                    {
                        if (!TrySaveViewModel())
                        {
                            return false;
                        }

                        MaybeSave(true);

                        if (CurrentViewModel != viewModel)
                        {
                            throw new InvalidOperationException();
                        }

                        ViewModelList.RemoveAt(ViewModelList.Count - 1);
                    }
                    break;

                default:
                    throw new NotSupportedException();
            }

            return true;
        }

        private void SetMainRows() => mainViewModel.MainRows = Row.Filter((ViewModelList[0] as RowViewModelBase)?.RowManager.Rows, r => (r as IRuleRow)?.Output is not null);

        private bool TrySaveViewModel() => CurrentViewModel?.TrySave().IsSuccess != false;
    }
}