using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Utils.Async;
using WPFApp.Controls.Wrappers.PatternWrappers;

namespace WPFApp.ViewModels
{
	public abstract class ImagePatternViewModel : ViewModelBase
	{
		private static readonly AsyncJobQueue asyncJobQueue = new();

		private string text;

		private CancellationTokenSource individualCancellationTokenSource;

		private BitmapImage imageSource;

		public virtual string Text
		{
			get => text;

			set
			{
				SetProperty(ref text, value);
				SetImageSource();
			}
		}

		public BitmapImage ImageSource
		{
			get => imageSource;
			set => SetProperty(ref imageSource, value);
		}

		public abstract IEnumerable<string> TypeableNames { get; }

		protected abstract string GetImgXPathFromTableXPath(string tableXPath, string typeableName);

		protected virtual string GetTypeableNameFromDisplayName(string displayName) => displayName;

		private static string GetFileName(string typeableName) => Info.InvalidFileNameCharsRegex.Replace(typeableName, string.Empty) + ".png";

		private static string GetWikiPageUrl(string typeableName) => "https://riskofrain2.fandom.com/wiki/" + typeableName.Replace(" ", "_");

		private async Task<string> GetImageUrlAsync(string typeableName, CancellationToken cancellationToken = default)
		{
			var client = PatternWrapper.RequestHtmlWeb();

			if (client is null)
			{
				return null;
			}

			string wikiPageUrl = GetWikiPageUrl(typeableName);
			HtmlNode document;
			try
			{
				CancellationTokenSource timeoutTokenSource = new(TimeSpan.FromSeconds(10));
				CancellationToken combinedToken = CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token, cancellationToken).Token;
				document = (await client.LoadFromWebAsync(wikiPageUrl, combinedToken)).DocumentNode;
			}
			catch (Exception)
			{
				return null;
			}

			HtmlNode table = document.SelectSingleNode("//table[@class='infoboxtable']");

			if (table is null)
			{
				return null;
			}

			HtmlNode img = document.SelectSingleNode(GetImgXPathFromTableXPath(table.XPath, typeableName));

			if (img is null)
			{
				return null;
			}

			return img.GetAttributeValue("data-src", null) ?? img.GetAttributeValue("src", null);
		}

		private void SetImageSource()
		{
			individualCancellationTokenSource?.Cancel();
			individualCancellationTokenSource = new();
			asyncJobQueue.WaitForMyJobAsync(new CancellableTask(token => SetImageSourceAsync(token)), individualCancellationTokenSource.Token);
		}

		private async Task SetImageSourceAsync(CancellationToken cancellationToken)
		{
			string typeableName = GetTypeableNameFromDisplayName(Text);
			if (!string.IsNullOrEmpty(typeableName))
			{
				string filePath = Path.Combine(Images.CacheLocation, GetFileName(typeableName));

				if (File.Exists(filePath))
				{
					ImageSource = Images.BuildFromUri(filePath);
					Images.CacheUpdated += SetImageSource;
					return;
				}
				else
				{
					string sourceString = await GetImageUrlAsync(typeableName, cancellationToken);
					if (sourceString is not null)
					{
						Uri sourceUri = new(sourceString);
						ImageSource = Images.BuildFromUri(sourceUri);
						Images.CacheUpdated += SetImageSource;
						await HttpHelper.DownloadFileAsync(sourceUri, filePath);
						return;
					}
				}
			}

			ImageSource = null;
			Images.CacheUpdated -= SetImageSource;
			return;
		}
	}

	public abstract class ImagePatternViewModel<T> : ImagePatternViewModel
	{
		private string text;

		public override IEnumerable<string> TypeableNames => AllowedValues.Select(GetTypeableName);

		public abstract IEnumerable<T> AllowedValues { get; }

		public override string Text
		{
			get => base.Text;

			set
			{
				if (text != value)
				{
					text = value;
					T foundValue = AllowedValues.FirstOrDefault(x => string.Equals(value, GetTypeableName(x), StringComparison.OrdinalIgnoreCase));
					base.Text = GetDisplayName(foundValue) ?? value;
				}
			}
		}

		public virtual string GetTypeableName(T value) => GetDisplayName(value);

		public abstract string GetDisplayName(T value);

		protected override string GetTypeableNameFromDisplayName(string displayName) => GetTypeableName(AllowedValues.FirstOrDefault(x => GetDisplayName(x) == displayName));
	}
}