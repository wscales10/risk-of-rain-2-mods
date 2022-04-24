using Rules.RuleTypes.Mutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using WPFApp.Controls;
using Utils.Async;

namespace WPFApp
{
	public partial class App
	{
		private readonly CancellationTokenSource exportCancellationTokenSource = new();

		private readonly TaskMachine taskMachine = new();

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

			MessageBoxResult result = MessageBox.Show(
				$"Export everything? (Select No to export only this {CurrentControl.ItemTypeName} and its descendants)",
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

		private void ExportToFile(IXmlControl xmlControl, string fileName) => taskMachine.Ingest(() => ExportAsync(xmlControl.GetContentXml(), fileName));

		private Task ExportAsync(XElement xml, string fileName)
		{
			var stream = new FileStream(fileName, FileMode.Create);
			return xml.SaveAsync(stream, SaveOptions.None, exportCancellationTokenSource.Token);
		}
	}
}