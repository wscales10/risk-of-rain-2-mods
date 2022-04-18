using HtmlAgilityPack;
using MyRoR2;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Utils;

namespace WPFApp.Controls.Wrappers.PatternWrappers
{
	internal class ScenePatternWrapper : ValuePatternWrapper<ScenePattern, Grid>
	{
		private readonly Image image = new() { Margin = new Thickness(4, 4, 4, 2) };

		public ScenePatternWrapper(ScenePattern pattern) : base(pattern)
		{
			UIElement.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
			UIElement.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			UIElement.Children.Add(image);
			TextBox.TextChanged += TextBox_TextChanged;
			_ = UIElement.Children.Add(TextBox);
			Grid.SetRow(TextBox, 1);
		}

		public override Grid UIElement { get; } = new();

		protected override string GetTextBoxText() => Pattern.DisplayName;

		protected override void DefineWith(string textBoxText) => Pattern.DefineWithDisplayName(textBoxText);

		protected override bool Validate(ScenePattern value) => value.Definition is not null;

		protected override void Display()
		{
			base.Display();
			_ = SetImageSource(TextBox.Text);
		}

		private static string GetFileName(string sceneDisplayName) => Info.InvalidFilePathCharsRegex.Replace(sceneDisplayName, string.Empty) + ".png";

		private static async Task<string> GetImageUrl(string sceneDisplayName)
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

			var table = document.SelectSingleNode("//table[@class='infoboxtable']");
			var img = table?.SelectSingleNode($"//img[@data-image-key='{WebUtility.UrlEncode(sceneDisplayName.Replace(' ', '_'))}.png']");

			if (img is null)
			{
				return null;
			}

			return img.GetAttributeValue("data-src", null) ?? img.GetAttributeValue("src", null);
		}

		private static string GetWikiPageUrl(string sceneDisplayName) => "https://riskofrain2.fandom.com/wiki/" + sceneDisplayName.Replace(" ", "_");

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e) => _ = SetImageSource(TextBox.Text);

		private async Task SetImageSource(string sceneDisplayName)
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
				string sourceString = await GetImageUrl(sceneDisplayName);
				if (sourceString is not null)
				{
					Uri sourceUri = new(sourceString);
					image.Source = HelperMethods.ImageFromUri(sourceUri);
					using WebClient client = new();
					await client.DownloadFileTaskAsync(sourceUri, filePath);
				}
			}
		}
	}
}