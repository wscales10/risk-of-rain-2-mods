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
using WPFApp.Views;

namespace WPFApp
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private readonly List<Navigation> history = new();

		private readonly MutableNavigationContext navigationContext = new();

		// private readonly ChangeProperty<bool, bool> OfflineMode = new(Settings.Default.OfflineMode);

		private int historyIndex;

		public App()
		{
			MainView = new(NavigationContext);
			MetadataClient = new SpotifyMetadataClient(x => MetadataClient.Log(x));
			MetadataClient.OnError += SpotifyClient_OnError;
			PlaybackClient = new SpotifyPlaybackClient(x => PlaybackClient.Log(x));
			PlaybackClient.OnError += SpotifyClient_OnError;
			Authorisation = new Authorisation(Scopes.Metadata.Concat(Scopes.Playback), logger: x => Authorisation.Log(x));
			navigationContext.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(navigationContext.IsOffline))
				{
					// TODO: Authorisation needs improving so it stops if it fails to connect and it can be restarted at any time. There should be a public function to manually request a refreshed token or restart the process.
					if (navigationContext.IsOffline)
					{
						_ = Authorisation.PauseAsync();
					}
					else
					{
						if (Authorisation.Lifecycle.IsRunning)
						{
							Authorisation.Resume();
						}
						else
						{
							Authorisation.InitiateScopeRequest();
						}

						SpotifyItemPicker.Refresh();
					}
				}
			};
		}

		public Authorisation Authorisation { get; }

		public SpotifyMetadataClient MetadataClient { get; }

		public SpotifyPlaybackClient PlaybackClient { get; }

		public List<ControlBase> ControlList { get; set; } = new();

		public ControlBase CurrentControl => ControlList.LastOrDefault();

		public MainView MainView { get; }

		public NavigationContext NavigationContext => new(navigationContext);

		public void GoBack()
		{
			Undo();
			display();
		}

		public void GoForward()
		{
			Redo();
			display();
		}

		public void GoHome()
		{
			Try(new RemoveNavigation(ControlList.Skip(1).Reverse()));
			display();
		}

		public ControlBase Display(object item)
		{
			ControlBase control = ControlList.Find(c => c.Object == item) ?? item switch
			{
				Rule rule => GetRuleControl(rule),
				IPattern pattern => GetPatternControl(pattern),
				_ => null,
			};

			if (Try(new AddNavigation(control)))
			{
				display();
				return CurrentControl;
			}

			return null;
		}

		public void GoUp()
		{
			goUp();
			display();
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			SpotifyItemPicker.OnMusicItemInfoRequested += HandleMusicItemInfoRequest;
			PatternWrapper.OnHtmlWebRequested += HandleHtmlWebRequest;
			BucketRow.OnCommandPreviewRequested += HandleCommandPreviewRequest;
			MainView.OnGoHome += GoHome;
			MainView.OnGoUp += GoUp;
			MainView.OnGoBack += GoBack;
			MainView.OnGoForward += GoForward;
			MainView.OnImportFile += ImportFile;
			MainView.OnExportFile += ExportToFile;
			MainView.OnReset += () => Reset();
			navigationContext.OnGoInto += Display;
			ImportXml(Rules.Examples.MimicRule.ToXml());

			//ImportXml(new IfRule(Query.Create("SceneName", StringPattern.Equals("foo")) | Query.Create("SceneName", StringPattern.Equals("bar")), new Bucket()).ToXml());
			display();
			MainView.Show();

			//new ControlTestView().Show();

			Authorisation.OnAccessTokenReceived += (_, t) =>
			{
				MetadataClient.GiftNewAccessToken(t);
				PlaybackClient.GiftNewAccessToken(t);
			};
			Authorisation.OnClientRequested += Web.Goto;
			if (!navigationContext.IsOffline)
			{
				Authorisation.InitiateScopeRequest();
			}
		}

		private void SpotifyClient_OnError(Exception e) => MessageBox.Show(
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

			ClearHistory();
			display();
		}

		private void ClearHistory()
		{
			history.Clear();
			historyIndex = 0;
		}

		private void display()
		{
			MainView.UpdateNavigationButtons(ControlList.Count < 2, historyIndex, history.Count);
			MainView.Control = CurrentControl;
		}

		private RuleControlBase GetRuleControl(Rule rule)
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

		private void goUp() => Try(new RemoveNavigation(ControlList.Last()));

		private async Task HandleMusicItemInfoRequest(SpotifyItem item, Func<MusicItemInfo, Task> callback)
		{
			if (navigationContext.IsOffline)
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

		private HtmlWeb HandleHtmlWebRequest() => navigationContext.IsOffline ? null : new HtmlWeb();

		private async Task HandleCommandPreviewRequest(Command command)
		{
			if (navigationContext.IsOffline)
			{
				return;
			}

			await PlaybackClient.Do(command);
		}

		private void PingSpotify()
		{
			Ping ping = new();
			var pingReply = ping.Send("accounts.spotify.com");
			navigationContext.IsOffline = pingReply.Status != IPStatus.Success;
		}

		private void Redo(int count = 1)
		{
			for (int i = 0; i < count && historyIndex < history.Count; i++)
			{
				if (!TryInner(history[historyIndex]))
				{
					break;
				}

				historyIndex++;
			}
		}

		private bool Try(Navigation navigation)
		{
			if (TryInner(navigation))
			{
				history.RemoveRange(historyIndex, history.Count - historyIndex);

				if (MainView.Control is not null)
				{
					history.Add(navigation);
					historyIndex++;
				}

				return true;
			}

			return false;
		}

		private bool TryInner(Navigation navigation)
		{
			switch (navigation)
			{
				case AddNavigation:
					foreach (var control in navigation.Controls)
					{
						if (!TryLeaveControl())
						{
							return false;
						}

						ControlList.Add(control);
					}
					break;

				case RemoveNavigation:
					foreach (var control in navigation.Controls)
					{
						if (!TryLeaveControl())
						{
							return false;
						}

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

		private bool TryLeaveControl()
		{
			var currentControl = CurrentControl;

			if (currentControl is null)
			{
				return true;
			}

			return currentControl.TryExit();
		}

		private void Undo(int count = 1)
		{
			for (int i = 0; i < count && historyIndex > 0; i++)
			{
				if (!TryInner(history[historyIndex - 1].Reverse))
				{
					break;
				}

				historyIndex--;
			}
		}
	}
}