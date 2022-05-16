using HtmlAgilityPack;
using MyRoR2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Utils.Async;
using WPFApp.Controls.Wrappers.PatternWrappers;

namespace WPFApp.ViewModels
{
    public class ScenePatternViewModel : ImagePatternViewModel
    {
        public override IEnumerable<string> Names { get; } = DefinedScene.GetAll().Select(s => s.DisplayName);

        protected override string GetImgXPathFromTableXPath(string tableXPath, string displayName) => $"{tableXPath}//img[@data-image-key='{WebUtility.UrlEncode(displayName.Replace(' ', '_'))}.png']";
    }

    public class EntityPatternViewModel : ImagePatternViewModel
    {
        public override IEnumerable<string> Names { get; } = DefinedEntity.GetAll().Select(s => s.DisplayName);

        protected override string GetImgXPathFromTableXPath(string tableXPath, string displayName) => $"{tableXPath}//img[contains(@data-image-name, '{displayName}')]";
    }

    public abstract class ImagePatternViewModel : ViewModelBase
    {
        private static readonly TaskMachine taskMachine = new SeniorTaskMachine();

        private CancellationTokenSource individualCancellationTokenSource;

        private BitmapImage imageSource;

        private string text;

        public abstract IEnumerable<string> Names { get; }

        public BitmapImage ImageSource
        {
            get => imageSource;
            set => SetProperty(ref imageSource, value);
        }

        public string Text
        {
            get => text;

            set
            {
                // TODO: decide how to handle Void Fiend and Voidling
                string fixedText = Names.FirstOrDefault(s => string.Equals(value, s, StringComparison.OrdinalIgnoreCase));
                text = fixedText ?? value;
                NotifyPropertyChanged();
                SetImageSource();
            }
        }

        protected abstract string GetImgXPathFromTableXPath(string tableXPath, string displayName);

        private static string GetFileName(string displayName) => Info.InvalidFileNameCharsRegex.Replace(displayName, string.Empty) + ".png";

        private static string GetWikiPageUrl(string displayName) => "https://riskofrain2.fandom.com/wiki/" + displayName.Replace(" ", "_");

        private void SetImageSource()
        {
            individualCancellationTokenSource?.Cancel();
            individualCancellationTokenSource = new();
            taskMachine.TryIngest(token => SetImageSourceAsync(token), individualCancellationTokenSource.Token);
        }

        private async Task SetImageSourceAsync(CancellationToken cancellationToken)
        {
            string displayName = Text;
            if (!string.IsNullOrEmpty(displayName))
            {
                string filePath = Path.Combine(Images.CacheLocation, GetFileName(displayName));

                if (File.Exists(filePath))
                {
                    ImageSource = Images.BuildFromUri(filePath);
                    Images.CacheUpdated += SetImageSource;
                    return;
                }
                else
                {
                    string sourceString = await GetImageUrlAsync(displayName, cancellationToken);
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

        private async Task<string> GetImageUrlAsync(string displayName, CancellationToken cancellationToken = default)
        {
            var client = PatternWrapper.RequestHtmlWeb();

            if (client is null)
            {
                return null;
            }

            string wikiPageUrl = GetWikiPageUrl(displayName);
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

            HtmlNode img = document.SelectSingleNode(GetImgXPathFromTableXPath(table.XPath, displayName));

            if (img is null)
            {
                return null;
            }

            return img.GetAttributeValue("data-src", null) ?? img.GetAttributeValue("src", null);
        }
    }
}