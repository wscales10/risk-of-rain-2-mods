using Rules.RuleTypes.Mutable;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using WPFApp.Controls;

namespace WPFApp
{
	public partial class App
	{
		private readonly CancellationTokenSource exportCancellationTokenSource = new();

		private Task exportTask;

		public void ImportFile(string filename)
		{
			var xml = XElement.Load(filename);
			ImportXml(xml);
		}

		public void ImportXml(XElement xml) => Reset(GetRuleControl(Rule.FromXml(xml)));

		private void ExportToFile(string fileName) => ExportToFile(GetControlForExport(), fileName);

		private IXmlControl GetControlForExport()
		{
			var masterXmlControl = ControlList[0] as IXmlControl;

			if (ControlList.Count < 2)
			{
				return masterXmlControl;
			}

			if (CurrentControl is not IXmlControl currentXmlControl)
			{
				return masterXmlControl;
			}

			var result = MessageBox.Show(MainView, $"Export everything? (Select No to export only this {CurrentControl.ItemTypeName} and its descendants)", "Export All?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);

			return result switch
			{
				MessageBoxResult.Yes => masterXmlControl,
				MessageBoxResult.No => currentXmlControl,
				_ => null,
			};
		}

		private void ExportToFile(IXmlControl xmlControl, string fileName)
		{
			if (exportTask is not null)
			{
				if (MessageBox.Show("Previous export is still in progress. Would you like to cancel it and continue with this export?") != MessageBoxResult.OK)
				{
					return;
				}

				if (MessageBox.Show("Are you sure you would like to cancel the previous export?") != MessageBoxResult.Yes)
				{
					return;
				}

				if (MessageBox.Show("Are you sure you're sure?") != MessageBoxResult.Yes)
				{
					return;
				}

				exportCancellationTokenSource.Cancel();
			}

			_ = ExportAsync(xmlControl.GetContentXml(), fileName);
		}

		private async Task ExportAsync(XElement xml, string fileName)
		{
			var stream = new FileStream(fileName, FileMode.Create);
			exportTask = xml.SaveAsync(stream, SaveOptions.None, exportCancellationTokenSource.Token);
			await exportTask;
			exportTask = null;
		}
	}
}