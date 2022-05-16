using Rules.RuleTypes.Mutable;
using Spotify;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using Utils.Async;
using WPFApp.Properties;
using WPFApp.ViewModels;

namespace WPFApp
{
    public partial class App
    {
        private readonly CancellationTokenSource exportCancellationTokenSource = new();

        private readonly TaskMachine exportTaskMachine;

        public void ImportFile(string fileName)
        {
            FileInfo fileInfo = new(fileName);
            var xml = XElement.Load(fileName);
            AutosaveLocation = fileInfo;
            ImportXml(xml, false);

            var playlistsFile = fileInfo.Directory.GetFiles("playlists.xml").SingleOrDefault();

            if (playlistsFile is not null)
            {
                Info.Playlists.Clear();

                foreach (var playlist in XElement.Load(playlistsFile.FullName).Elements().Select(x => new Playlist(x)))
                {
                    Info.Playlists.Add(playlist);
                }
            }
        }

        public void ImportXml(XElement xml, bool resetAutosave = true) => Reset(viewModels[Rule.FromXml(xml)], resetAutosave);

        private static FileStream GetWriteStream(string path, int timeoutMs)
        {
            var time = Stopwatch.StartNew();
            while (time.ElapsedMilliseconds < timeoutMs)
            {
                try
                {
                    return new FileStream(path, FileMode.Create, FileAccess.Write);
                }
                catch (IOException e) when (e.HResult == -2147024864)
                {
                }
            }

            throw new TimeoutException($"Failed to get a write handle to {path} within {timeoutMs}ms.");
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

        private void Export(XElement xml, FileInfo fileName)
        {
            using FileStream fs = new(fileName.FullName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            xml?.Save(fs);
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

        private void ExportToFile(IXmlViewModel xmlControl, FileInfo fileName)
        {
            // Using synchronous code because async is a bitch to work with taskMachine.Ingest(() =>
            // ExportAsync(xmlControl.GetContentXml(), fileName));
            XElement xml;
            try
            {
                xml = xmlControl.GetContentXml();
            }
            catch (XmlException)
            {
                _ = MessageBox.Show("Export error.");
                return;
            }

            Export(xml, fileName);
            ExportPlaylists(fileName.Directory);
        }

        private void ExportPlaylists(DirectoryInfo directoryInfo)
        {
            XElement element = new("Playlists");

            foreach (var playlist in Info.Playlists)
            {
                element.Add(playlist.ToXml());
            }

            Export(element, new(Path.Combine(directoryInfo.FullName, "playlists.xml")));
        }

        private Task ExportAsync(XElement xml, string fileName)
        {
            var stream = GetWriteStream(fileName, 60000);
            return xml.SaveAsync(stream, SaveOptions.None, exportCancellationTokenSource.Token);
        }
    }
}