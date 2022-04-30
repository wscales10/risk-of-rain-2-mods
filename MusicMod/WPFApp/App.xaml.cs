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
using WPFApp.Controls.PatternControls;
using WPFApp.Controls.Rows;
using WPFApp.Controls.RuleControls;
using WPFApp.Controls.Wrappers.PatternWrappers;
using WPFApp.Properties;
using WPFApp.Views;
using System.Diagnostics.CodeAnalysis;
using WPFApp.ViewModels;
using System.Collections;
using System.ComponentModel;

namespace WPFApp
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private readonly History<Navigation> history = new();

		private readonly MutableNavigationContext navigationContext = new();

		private readonly Cache<object, ControlBase> controls;

		private Action display;

		private string autosaveLocation;

		private MainViewModel mainViewModel;

		public App()
		{
			controls = new(item => item switch
			{
				Rule rule => GetRuleControl(rule),
				IPattern pattern => GetPatternControl(pattern),
				_ => null,
			});

			navigationContext.OnGoHome += GoHome;
			navigationContext.OnGoUp += GoUp;
			navigationContext.OnGoInto += Display;
			navigationContext.OnControlRequested += (obj) =>
			{
				return obj is null ? null : controls[obj];
			};
			NavigationContext = new(navigationContext);
			history.ActionRequested += TryInner;
			MetadataClient = new SpotifyMetadataClient(x => MetadataClient.Log(x));
			MetadataClient.OnError += SpotifyClient_OnError;
			PlaybackClient = new SpotifyPlaybackClient(x => PlaybackClient.Log(x));
			PlaybackClient.OnError += SpotifyClient_OnError;
			Authorisation = new Authorisation(Scopes.Metadata.Concat(Scopes.Playback), logger: x => Authorisation.Log(x));
			Settings.Default.PropertyChanged += OnSettingChanged;
		}

		private event Func<string> OnAutosaveLocationRequested;

		public NavigationContext NavigationContext { get; }

		public Authorisation Authorisation { get; }

		public SpotifyMetadataClient MetadataClient { get; }

		public SpotifyPlaybackClient PlaybackClient { get; }

		public List<ControlBase> ControlList { get; set; } = new();

		public ControlBase CurrentControl => ControlList.LastOrDefault();

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
			_ = Try(new RemoveNavigation(ControlList.Skip(1).Reverse()));
			display();
		}

		public ControlBase Display(IEnumerable list)
		{
			ControlBase output = null;
			foreach (object item in list)
			{
				ControlBase control = (item as ControlBase) ?? (item is null ? null : controls[item]);

				if (Try(new AddNavigation(control)))
				{
					display();
					output = CurrentControl;
				}
				else
				{
					break;
				}
			}

			display();
			return output;
		}

		public void GoUp(int count)
		{
			_ = Try(new RemoveNavigation(ControlList.Reverse<ControlBase>().Take(count)));
			display();
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			MainView mainView = new(NavigationContext);
			Attach(mainView);
			RegisterRequestHandlers();
			ImportXml(Rules.Examples.MimicRule.ToXml());
			display();
			mainView.Show();

			//new ControlTestView().Show();

			Authorisation.OnAccessTokenReceived += (_, t) =>
			{
				MetadataClient.GiftNewAccessToken(t);
				PlaybackClient.GiftNewAccessToken(t);
				SpotifyItemPicker.Refresh();
				Settings.Default.Save();
			};
			Authorisation.OnClientRequested += Web.Goto;
			if (!Settings.Default.OfflineMode)
			{
				Authorisation.InitiateScopeRequest();
			}
		}

		private void RegisterRequestHandlers()
		{
			SpotifyItemPicker.OnMusicItemInfoRequested += HandleMusicItemInfoRequestAsync;
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
				navigationContext.IsHome = ControlList.Count < 2;
				mainViewModel.BackCommand.CanExecute = history.CurrentIndex > 0;
				mainViewModel.ForwardCommand.CanExecute = history.ReverseIndex > 0;
				mainViewModel.Control = CurrentControl;
			};
		}

		[SuppressMessage("Usage", "VSTHRD001:Avoid legacy thread switching APIs", Justification = "It works, the new one doesn't")]
		private bool TryClose()
		{
			MaybeSave();

			if (taskMachine.IsRunning)
			{
				ExportWindow exportWindow = new();
				_ = taskMachine.Lifecycle.ContinueWith(_ => Dispatcher.Invoke(() =>
				{
					exportWindow.Close();
					Shutdown();
				}), TaskScheduler.Default);
				exportWindow.Show();
				return false;
			}

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
			if (e.PropertyName == nameof(Settings.OfflineMode))
			{
				// TODO: Authorisation needs improving so it stops if it fails to connect and it can be restarted at any time. There should be a public function to manually request a refreshed token or restart the process.
				if (settings.OfflineMode)
				{
					_ = Authorisation.PauseAsync();
				}
				else if (Authorisation.Lifecycle.IsRunning)
				{
					Authorisation.Resume();
					SpotifyItemPicker.Refresh();
				}
				else
				{
					Authorisation.InitiateScopeRequest();
				}
			}
		}

		private void SpotifyClient_OnError(Exception e) => _ = MessageBox.Show(
			e.Message,
			Utils.HelperMethods.AddSpacesToPascalCaseString(e.GetType().Name) + " (Spotify Client)",
			MessageBoxButton.OK,
			MessageBoxImage.Error);

		private PatternControlBase GetPatternControl(IPattern pattern)
		{
			if (pattern is IListPattern lp)
			{
				return new ListPatternControl(lp, lp.ValueType, NavigationContext);
			}

			return null;
		}

		private void Reset(ControlBase control = null)
		{
			ControlList.Clear();

			if (control is not null)
			{
				ControlList.Add(control);
			}
			else
			{
				mainViewModel.MainRows = null;
			}

			history.Clear();
			display();
		}

		private ControlBase GetRuleControl(Rule rule)
		{
			return new RuleControlWrapper(rule switch
			{
				StaticSwitchRule sr => new SwitchRuleControl(sr, NavigationContext),
				ArrayRule ar => new ArrayRuleControl(ar, NavigationContext),
				IfRule ir => new IfRuleControl(ir, NavigationContext),
				Bucket b => new BucketControl(b, NavigationContext),
				_ => null,
			});
		}

		private async Task HandleMusicItemInfoRequestAsync(SpotifyItem item, Func<MusicItemInfo, Task> callback)
		{
			if (Settings.Default.OfflineMode || !MetadataClient.IsAuthorised)
			{
				await callback(null);
				return;
			}

			GetMusicItemInfoCommand command = new(item, callback);
			await MetadataClient.Do(command);
			if (command.Task is not null)
			{
				await command.Task;
			}
		}

		private HtmlWeb HandleHtmlWebRequest() => Settings.Default.OfflineMode ? null : new HtmlWeb();

		private async Task HandleCommandPreviewRequestAsync(Command command)
		{
			if (Settings.Default.OfflineMode)
			{
				return;
			}

			await PlaybackClient.Do(command);
		}

		private void PingSpotify()
		{
			Ping ping = new();
			var pingReply = ping.Send("accounts.spotify.com");
			Settings.Default.OfflineMode = pingReply.Status != IPStatus.Success;
		}

		private bool Try(Navigation navigation) => history.Try(navigation, CurrentControl is not null);

		private void MaybeSave()
		{
			if (Settings.Default.Autosave && !TrySave())
			{
				Settings.Default.Autosave = false;
			}
		}

		private bool TrySave()
		{
			if (ControlList[0] is not IXmlControl xmlControl)
			{
				return false;
			}

			if (!CurrentControl.TrySave().IsSuccess)
			{
				return false;
			}

			if ((autosaveLocation ??= OnAutosaveLocationRequested?.Invoke()) is null)
			{
				return false;
			}

			ExportToFile(xmlControl, autosaveLocation);
			return true;
		}

		private bool TryInner(Navigation navigation)
		{
			switch (navigation)
			{
				case AddNavigation:
					foreach (ControlBase control in navigation.Controls)
					{
						if (!TryLeaveControl())
						{
							return false;
						}

						MaybeSave();

						if (ControlList.Count == 0)
						{
							mainViewModel.MainRows = Row.Filter((control as IRowControl)?.RowManager.Rows, r => (r as IRuleRow)?.Output is not null);
						}

						ControlList.Add(control);
					}
					break;

				case RemoveNavigation:
					foreach (ControlBase control in navigation.Controls)
					{
						if (!TryLeaveControl())
						{
							return false;
						}

						MaybeSave();

						if (CurrentControl != control)
						{
							throw new InvalidOperationException();
						}

						ControlList.RemoveAt(ControlList.Count - 1);
					}
					break;

				default:
					throw new NotSupportedException();
			}

			return true;
		}

		private bool TryLeaveControl() => CurrentControl?.TrySave().IsSuccess != false;
	}
}