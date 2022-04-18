using Spotify;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Utils;
using WPFApp.Properties;

namespace WPFApp.Controls
{
	/// <summary>
	/// Interaction logic for SpotifyItemPicker.xaml
	/// </summary>
	public partial class SpotifyItemPicker : UserControl
	{
		private static readonly Regex regex = new(@"https?:\/\/open.spotify.com\/(?<itemType>.*?)\/(?<id>\w*)");
		private SpotifyItem? item;
		private readonly SetProperty<MusicItemInfo> Info = new();
		private static readonly Brush infoBrush = new SolidColorBrush(Color.FromRgb(244, 244, 244));

		public SpotifyItemPicker()
		{
			PreviewMouseLeftButtonUp += (s, e) =>
			{
				var info = (s as SpotifyItemPicker)?.Info.Get();
				if (info is not null)
				{
					Web.Goto(GetUri(info.PreviewItem));
				}
			};
			InitializeComponent();
			OnConnectionMade += RequestMusicItemInfo;
		}

		public void RequestMusicItemInfo()
		{
			if (Item is SpotifyItem si)
			{
				Cursor = Cursors.Hand;
				_ = (OnMusicItemInfoRequested?.Invoke(si, DisplayMusicItemInfo));
			}
			else
			{
				Cursor = null;
				_ = DisplayMusicItemInfo(null);
			}
		}

		public SpotifyItem? Item
		{
			get => item;

			set
			{
				item = value;
				RequestMusicItemInfo();
			}
		}

		internal static event MusicItemInfoRequestHandler OnMusicItemInfoRequested;

		private static event Action OnConnectionMade;

		internal static void Refresh()
		{
			OnConnectionMade?.Invoke();
		}

		private void Border_PreviewDrop(object sender, DragEventArgs e)
		{
			object dataObject = e?.Data?.GetData(typeof(string));

			if (dataObject is null || dataObject is not string url)
			{
				this.Log("No data");
				return;
			}

			Match match = regex.Match(url);

			if (!match.Success)
			{
				this.Log("Unrecognised url format");
				return;
			}

			string itemType = match.Groups["itemType"]?.Value;
			string id = match.Groups["id"]?.Value;

			Item = new(itemType.AsEnum<SpotifyItemType>(true), id);
		}

		private Task DisplayMusicItemInfo(MusicItemInfo info)
		{
			Info.Set(info);
			void AddLabel(string text, SpotifyItem? linkedItem = null)
			{
				TextBlock output = new()
				{
					Text = text,
					Foreground = infoBrush,
					FontSize = 14,
					Padding = new Thickness(0),
					Margin = new Thickness(0),
				};

				if (linkedItem is SpotifyItem si)
				{
					output.Cursor = Cursors.Hand;
					output.MouseEnter += (s, e) => output.TextDecorations = TextDecorations.Underline;
					output.MouseLeave += (s, e) => output.TextDecorations = null;
					output.PreviewMouseLeftButtonUp += (s, e) =>
					{
						Web.Goto(GetUri(si));
						e.Handled = true;
					};
				}

				infoPanel.Children.Add(output);
			}

			infoPanel.Children.Clear();
			if (info is null)
			{
				nameLabel.Content = Item?.Id ?? "Drop Spotify item here";
				if (Item is SpotifyItem si)
				{
					AddLabel(si.Type.ToString());
				}
			}
			else
			{
				nameLabel.Content = info.Name;
				AddLabel(info.MusicItem.Type.ToString());

				if (info.Creators?.Length > 0)
				{
					AddLabel(" - ");

					foreach ((string name, SpotifyItem item) in info.Creators)
					{
						AddLabel(name, item);
						AddLabel(", ");
					}

					infoPanel.Children.RemoveAt(infoPanel.Children.Count - 1);
				}

				string imageUrl = info.Images.OrderBy(i => i.Width * i.Height).FirstOrDefault()?.Url;
				image.Source = HelperMethods.ImageFromUri(imageUrl);
			}

			return Task.CompletedTask;
		}

		private static Uri GetUri(SpotifyItem item)
		{
			return Settings.Default.OpenLinksInApp ? item.GetUri() : item.GetUrl();
		}
	}

	public delegate Task MusicItemInfoRequestHandler(SpotifyItem item, Func<MusicItemInfo, Task> callback);
}
