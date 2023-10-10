using Rules.RuleTypes.Interfaces;
using Rules.RuleTypes.Mutable;
using Spotify;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using Utils;
using WPFApp.Properties;
using WPFApp.ViewModels;
using Utils.Reflection;
using Spotify.Commands;

namespace WPFApp
{
    public partial class App
    {
        public void ImportFile(string fileName)
        {
            this.Log("Attempting to import file " + fileName);
            FileInfo fileInfo = new(fileName);
            var xml = XElement.Load(fileName);

            var typePair = Info.TypePairs.SingleOrDefault(p => p.Item1.FullName == xml.Attribute("TContext")?.Value && p.Item2.FullName == xml.Attribute("TOut")?.Value);

            if (typePair == default)
            {
                MessageBox.Show("Import Error", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            typeof(App).GetMethod("ImportXml").MakeGenericMethod(typePair.Item1, typePair.Item2).Invoke(this, new object[] { xml, false });

            AutosaveLocation = fileInfo;

            if (playlistsController.IsEnabled)
            {
                var playlistsFile = fileInfo.Directory.GetFiles($"{Path.GetFileNameWithoutExtension(fileInfo.FullName)}.playlists.xml").SingleOrDefault()
                                    ?? fileInfo.Directory.GetFiles("playlists.xml").SingleOrDefault();

                if (playlistsFile is not null)
                {
                    playlistsController.Playlists.Clear();

                    foreach (var playlist in XElement.Load(playlistsFile.FullName).Elements().Select(x => new Playlist(x)))
                    {
                        playlistsController.Playlists.Add(playlist);
                    }
                }
            }
        }

        public void ImportXml<TContext, TOut>(XElement xml, bool resetAutosave = true) => SetRule(Info.GetRuleParser<TContext, TOut>().Parse(xml), resetAutosave);

        public void SetRule<TContext, TOut>(Rule<TContext, TOut> rule, bool resetAutosave = true)
        {
            Info.SetPatternParser<TContext>();
            playlistsController.IsEnabled = typeof(TOut) == typeof(ICommandList);
            Reset(viewModels[rule], resetAutosave);
        }

        public void ImportRule<TContext, TOut>(IRule<TContext, TOut> rule, bool resetAutosave = true) => ImportXml<TContext, TOut>(rule.ToXml(), resetAutosave);

        public void ImportRule(Type tContext, Type tOut, object rule, bool resetAutosave = true)
        {
            GetType().GetMethod("ImportRule", mi => mi.ContainsGenericParameters).MakeGenericMethod(tContext, tOut).Invoke(this, new[] { rule, resetAutosave });
        }

        private static void Export(XElement xml, FileInfo fileName)
        {
            using FileStream fs = new(fileName.FullName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            xml?.Save(fs);
        }

        private void ExportToFile(IXmlViewModel xmlViewModel, FileInfo fileInfo)
        {
            XElement xml;
            try
            {
                xml = xmlViewModel.GetContentXml();
            }
            catch (XmlException)
            {
                _ = MessageBox.Show("Export error.");
                return;
            }

            Export(xml, fileInfo);
            ExportPlaylists(fileInfo.Directory, Path.GetFileNameWithoutExtension(fileInfo.FullName));
        }

        private void ExportPlaylists(DirectoryInfo directoryInfo, string name = null)
        {
            XElement element = new("Playlists");

            foreach (var playlist in playlistsController.Playlists)
            {
                element.Add(playlist.ToXml());
            }

            Export(element, new(Path.Combine(directoryInfo.FullName, name is null ? "playlists.xml" : $"{name}.playlists.xml")));
        }

        private IXmlViewModel GetControlForExport()
        {
            var masterXmlControl = ViewModelList[0] as IXmlViewModel;

            if (ViewModelList.Count < 2)
            {
                return masterXmlControl;
            }

            if (CurrentViewModel is not IXmlViewModel currentXmlControl)
            {
                return masterXmlControl;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Export everything? (Select No to export only this {currentXmlControl.ItemTypeName} and its descendants)",
                "Export All?",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question,
                MessageBoxResult.Yes);

            return result switch
            {
                MessageBoxResult.Yes => masterXmlControl,
                MessageBoxResult.No => currentXmlControl,
                _ => null,
            };
        }

        private void ExportToFile(string fileName)
        {
            FileInfo fileInfo = new(fileName);

            if (CurrentViewModel.TrySave())
            {
                if (Settings.Default.Autosave)
                {
                    AutosaveLocation = fileInfo;
                }

                ExportToFile(GetControlForExport(), fileInfo);
            }
        }
    }
}