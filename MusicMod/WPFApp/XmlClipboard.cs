using System;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace WPFApp
{
	public class XmlClipboard : NotifyPropertyChangedBase
	{
		private Named<XElement> selectedItem;

		public XmlClipboard()
		{
			PasteCommand = new(_ => Choose(SelectedItem), this, nameof(SelectedItem), (x) => x is not null);
		}

		public event Action OnSelect;

		public ObservableCollection<Named<XElement>> Items { get; } = new();

		public XElement ChosenValue { get; private set; }

		public ButtonCommand PasteCommand { get; }

		public Named<XElement> SelectedItem { get => selectedItem; set => SetProperty(ref selectedItem, value); }

		public void Choose(Named<XElement> item)
		{
			if (item is null)
			{
				ChosenValue = null;
				return;
			}

			if (Items.Remove(item))
			{
				ChosenValue = item.Value;
			}
			else
			{
				throw new ArgumentOutOfRangeException(nameof(item), item, "not in collection");
			}

			OnSelect?.Invoke();
		}
	}
}