using HtmlAgilityPack;
using MyRoR2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utils;
using WPFApp.Controls.Wrappers.PatternWrappers;

namespace WPFApp.Controls.PatternControls
{
	/// <summary>
	/// Interaction logic for ScenePatternControl.xaml
	/// </summary>
	public partial class ScenePatternControl : UserControl
	{
		public ScenePatternControl() => InitializeComponent();

		public static IEnumerable<string> SceneNames { get; } = DefinedScene.GetAll().Select(s => s.DisplayName);

		public async Task SetImageSourceAsync(string sceneDisplayName)
		{
			if (sceneDisplayName is null)
			{
				image.Source = null;
				return;
			}

			string filePath = Path.Combine(Paths.AssemblyDirectory, "Images", GetFileName(sceneDisplayName));

			if (File.Exists(filePath))
			{
				image.Source = HelperMethods.ImageFromUri(filePath);
			}
			else
			{
				string sourceString = await GetImageUrlAsync(sceneDisplayName);
				if (sourceString is not null)
				{
					Uri sourceUri = new(sourceString);
					image.Source = HelperMethods.ImageFromUri(sourceUri);
					using WebClient client = new();
					await client.DownloadFileTaskAsync(sourceUri, filePath);
				}
			}
		}

		private static string GetFileName(string sceneDisplayName) => Info.InvalidFilePathCharsRegex.Replace(sceneDisplayName, string.Empty) + ".png";

		private static string GetWikiPageUrl(string sceneDisplayName) => "https://riskofrain2.fandom.com/wiki/" + sceneDisplayName.Replace(" ", "_");

		private static async Task<string> GetImageUrlAsync(string sceneDisplayName)
		{
			var client = PatternWrapper.RequestHtmlWeb();

			if (client is null)
			{
				return null;
			}

			string wikiPageUrl = GetWikiPageUrl(sceneDisplayName);
			HtmlNode document;
			try
			{
				document = (await client.LoadFromWebAsync(wikiPageUrl, new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token)).DocumentNode;
			}
			catch (Exception)
			{
				return null;
			}

			HtmlNode table = document.SelectSingleNode("//table[@class='infoboxtable']");
			HtmlNode img = table?.SelectSingleNode($"//img[@data-image-key='{WebUtility.UrlEncode(sceneDisplayName.Replace(' ', '_'))}.png']");

			if (img is null)
			{
				return null;
			}

			return img.GetAttributeValue("data-src", null) ?? img.GetAttributeValue("src", null);
		}

		private void textBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
		{
			string fixedText = SceneNames.FirstOrDefault(s => string.Equals(((TextBox)sender).Text, s, StringComparison.OrdinalIgnoreCase));
			if (fixedText is not null)
			{
				((TextBox)sender).Text = fixedText;
			}

			_ = SetImageSourceAsync(((TextBox)sender).Text);
		}
	}
}