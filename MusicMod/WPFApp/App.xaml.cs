using HtmlAgilityPack;
using Patterns;
using Patterns.Patterns;
using Rules.RuleTypes.Mutable;
using Spotify;
using Spotify.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Utils;
using WPFApp.Controls;
using WPFApp.Controls.Rows;
using WPFApp.Controls.Wrappers.PatternWrappers;
using WPFApp.Properties;
using WPFApp.Views;
using WPFApp.ViewModels;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Threading;
using System.Collections.ObjectModel;
using Utils.Async;

using System.Xml.Linq;
using Utils.Reflection;
using IPC;

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

		private readonly bool isTestMode = false;

		private readonly XmlClipboard clipboard = new();

		private readonly ClipboardWindow clipboardWindow;

		private readonly AuthorisationClient authorisationClient = new();

		private Action Render;

		private MainViewModel mainViewModel;

		public App()
		{
			if (isTestMode)
			{
				return;
			}

			clipboardWindow = new(clipboard);

			timer = new(_ => PlaybackClient.Stop());

			viewModels = new(item => item switch
			{
				RuleBase rule => GetRuleViewModel(rule),
				IPattern pattern => GetPatternViewModel(pattern),
				Playlist playlist => new PlaylistViewModel(playlist, NavigationContext),
				ObservableCollection<Playlist> playlists => new PlaylistsViewModel(playlists, NavigationContext),
				_ => null,
			});

			navigationContext.OnGoHome += GoHome;
			navigationContext.OnGoUp += GoUp;
			navigationContext.OnGoInto += Display;
			navigationContext.TreeNavigationRequested += NavigationContext_TreeNavigationRequested;
			navigationContext.ViewModelRequested += (obj) => obj is null ? null : viewModels[obj];
			navigationContext.ClipboardItemRequested += () =>
			{
				clipboardWindow.ShowDialog();
				return clipboard.ChosenValue;
			};
			NavigationContext = new(navigationContext);
			history.ActionRequested += TryInner;
			MetadataClient = new SpotifyMetadataClient(x => MetadataClient.Log(x), authorisationClient.Preferences);
			MetadataClient.OnError += SpotifyClient_OnError;
			PlaybackClient = new SpotifyPlaybackClient(Info.Playlists, x => PlaybackClient.Log(x), authorisationClient.Preferences);
			PlaybackClient.OnError += SpotifyClient_OnError;
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

		public SpotifyMetadataClient MetadataClient { get; }

		public SpotifyPlaybackClient PlaybackClient { get; }

		public List<NavigationViewModelBase> ViewModelList { get; set; } = new();

		public NavigationViewModelBase CurrentViewModel => ViewModelList.LastOrDefault();

		internal static AsyncManager AsyncManager { get; } = new();

		public static (Type, Type)? GetRuleType()
		{
			var view = new RuleTypePickerView();
			var result = view.ShowDialog();
			if (result == true)
			{
				return view.ViewModel.SelectedTypePair;
			}
			else
			{
				return null;
			}
		}

		public void GoBack()
		{
			_ = history.Undo();
			Render();
		}

		public void GoForward()
		{
			_ = history.Redo();
			Render();
		}

		public void GoHome()
		{
			_ = Try(goHome());
			Render();
		}

		public NavigationViewModelBase Display(object obj)
		{
			NavigationViewModelBase output = null;
			IEnumerable list = obj switch
			{
				RuleBase or IPattern or Playlist or IEnumerable<Playlist> or NavigationViewModelBase => new[] { obj },
				IEnumerable enumerable => enumerable,
				_ => throw new NotImplementedException(),
			};
			foreach (object item in list)
			{
				NavigationViewModelBase viewModel = (item as NavigationViewModelBase) ?? (item is null ? null : viewModels[item]);

				if (Try(new AddNavigation(viewModel)))
				{
					Render();
					output = CurrentViewModel;
				}
				else
				{
					Render();
					break;
				}
			}

			return output;
		}

		public bool GoUp(int count)
		{
			var result = Try(new RemoveNavigation(ViewModelList.Reverse<NavigationViewModelBase>().Take(count)));
			Render();
			return result;
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			if (isTestMode)
			{
				new ControlTestView().Show();
				return;
			}

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
				catch (FileNotFoundException ex)
				{
					Settings.Default.Autosave = false;
					MessageBox.Show(ex.Message);
				}
			}
			else
			{
				TryLoadExample();
			}

			authorisationClient.Preferences.PropertyChanged += (name) =>
			{
				if (name == nameof(IPreferences.AccessToken))
				{
					Current.Dispatcher.BeginInvoke(SpotifyItemPickerViewModel.Refresh);
				}
			};

			if (!Settings.Default.OfflineMode)
			{
				TryAuthorise();
			}

			Render();
			mainView.Show();
		}

		private static object GetExampleRule((Type, Type) ruleType)
		{
			if (ruleType == (typeof(MyRoR2.RoR2Context), typeof(string)))
			{
				return RuleExamples.RiskOfRain2.Ror2Rule.Instance;
			}
			else if (ruleType == (typeof(string), typeof(ICommandList)))
			{
				return RuleExamples.RiskOfRain2.MimicRule.Instance;
			}
			else
			{
				return null;
			}
		}

		private void TryLoadExample()
		{
			var ruleType = GetRuleType();

			if (ruleType is not null)
			{
				ImportRule(ruleType?.Item1, ruleType?.Item2, GetExampleRule(ruleType.Value));
			}
		}

		private void TryAuthorise()
		{
			var run = authorisationClient.TryStart.CreateRun();

			foreach (var progressUpdate in run.GetProgressUpdates())
			{
				switch (progressUpdate.Sender)
				{
					case IPC.Client:
						switch (progressUpdate.Args)
						{
							case Exception ex:
								run.Continue = false;
								var displayedException = ex is SendException ? ex.InnerException : ex;
								MessageBox.Show(displayedException.Message, displayedException.GetType().FullName);
								break;
						}
						break;

					default:
						throw new NotSupportedException();
				}
			}

			if (run.Result)
			{
				SpotifyItemPickerViewModel.Refresh();
			}
			else
			{
				Settings.Default.OfflineMode = true;
			}
		}

		private bool NavigationContext_TreeNavigationRequested(IRuleRow arg)
		{
			var node = arg;
			List<IRuleRow> rows = new();
			do
			{
				rows.Add(node);
				node = node.Parent;
			} while (node is not null);

			rows.Reverse();

			var navigation = new CompoundNavigation(new Navigation[] { goHome(), new AddNavigation(rows.Select(r => r.OutputViewModel)) });
			bool result = Try(navigation);
			Render();
			return result;
		}

		private RemoveNavigation goHome() => new(ViewModelList.Skip(1).Reverse());

		private void RegisterRequestHandlers()
		{
			SpotifyItemPicker.MusicItemInfoRequested += HandleMusicItemInfoRequestAsync;
			PatternWrapper.OnHtmlWebRequested += HandleHtmlWebRequest;
			CommandListRow.OnCommandPreviewRequested += HandleCommandPreviewRequestAsync;
		}

		private void Attach(MainView mainView)
		{
			mainViewModel = (MainViewModel)mainView.DataContext;
			mainViewModel.OnGoBack += GoBack;
			mainViewModel.OnGoForward += GoForward;
			mainViewModel.OnImportFile += ImportFile;
			mainViewModel.OnExportFile += ExportToFile;
			mainViewModel.OnCopy += CopyCurrentItemToClipboard;
			mainViewModel.OnReset += () => Reset();
			mainViewModel.OnExampleRequested += TryLoadExample;
			mainView.OnTryEnableAutosave += TryEnableAutosave;
			mainView.OnTryClose += TryClose;
			mainView.Loaded += (s, e) => clipboardWindow.Owner = (Window)s;
			OnAutosaveLocationRequested += MainView.GetExportLocation;

			Render = () =>
			{
				navigationContext.Path = string.Join(" > ", ViewModelList.Skip(1).Select(vm =>
				{
					var output = navigationContext.GetLabel(vm);
					output ??= (vm as IItemViewModel)?.ItemTypeName;
					output ??= "?";
					return output;
				}));
				navigationContext.IsHome = ViewModelList.Count < 2;
				mainViewModel.BackCommand.CanExecute = history.CurrentIndex > 0;
				mainViewModel.ForwardCommand.CanExecute = history.ReverseIndex > 0;
				mainViewModel.ItemViewModel = CurrentViewModel;
			};

			mainView.newRuleControl.TypesRequested += GetRuleType;
		}

		private void CopyCurrentItemToClipboard()
		{
			if (CurrentViewModel is IXmlViewModel viewModel)
			{
				var xml = viewModel.GetContentXml();
				clipboard.Items.Add(new(xml.ToString(SaveOptions.DisableFormatting), xml));
			}
		}

		private bool TryClose()
		{
			MaybeSave();
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

					if (!settings.OfflineMode)
					{
						TryAuthorise();
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

		private NavigationViewModelBase GetPatternViewModel(IPattern pattern)
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
			Render();
		}

		private NavigationViewModelBase GetRuleViewModel(RuleBase rule)
		{
			var ruleType = rule.GetType();

			if (ruleType.IsGenericType(typeof(StaticSwitchRule<,>)))
			{
				return (NavigationViewModelBase)typeof(SwitchRuleViewModel<,>).MakeGenericType(ruleType.GenericTypeArguments).Construct(rule, NavigationContext);
			}
			else if (ruleType.IsGenericType(typeof(ArrayRule<,>)))
			{
				return (NavigationViewModelBase)typeof(ArrayRuleViewModel<,>).MakeGenericType(ruleType.GenericTypeArguments).Construct(rule, NavigationContext);
			}
			else if (ruleType.IsGenericType(typeof(IfRule<,>)))
			{
				return (NavigationViewModelBase)typeof(IfRuleViewModel<,>).MakeGenericType(ruleType.GenericTypeArguments).Construct(rule, NavigationContext);
			}
			else if (ruleType.IsGenericType(typeof(Bucket<,>)) && ruleType.GenericTypeArguments[1] == typeof(ICommandList))
			{
				return (NavigationViewModelBase)typeof(CommandListBucketViewModel<>).MakeGenericType(ruleType.GenericTypeArguments[0]).Construct(rule, NavigationContext);
			}
			else
			{
				return null;
			}
		}

		private async Task<ConditionalValue<MusicItemInfo>> HandleMusicItemInfoRequestAsync(SpotifyItem item, CancellationToken? cancellationToken)
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
			timer.Change(TimeSpan.FromSeconds(10), Timeout.InfiniteTimeSpan);
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
			if (ViewModelList.FirstOrDefault() is not IXmlViewModel xmlViewModel)
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

			ExportToFile(xmlViewModel, AutosaveLocation);
			return true;
		}

		private bool TryInner(Navigation navigation)
		{
			switch (navigation)
			{
				case AddNavigation addNavigation:
					foreach (NavigationViewModelBase viewModel in addNavigation.ViewModels)
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

				case RemoveNavigation removeNavigation:
					foreach (NavigationViewModelBase viewModel in removeNavigation.ViewModels)
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

				case CompoundNavigation compoundNavigation:
					return compoundNavigation.Navigations.All(TryInner);

				default:
					throw new NotSupportedException();
			}

			return true;
		}

		private void SetMainRows() => mainViewModel.MainRows = Row.Filter((ViewModelList[0] as RowViewModelBase)?.RowManager.Rows, r => (r as IRuleRow)?.Output is not null);

		private bool TrySaveViewModel() => CurrentViewModel?.TrySave().IsSuccess != false;
	}
}