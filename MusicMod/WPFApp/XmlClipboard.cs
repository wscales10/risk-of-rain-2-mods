using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using Utils;

namespace WPFApp
{
    public class XmlClipboard : NotifyPropertyChangedBase
    {
        private Named<XElement> selectedItem;

        private Named<XElement> externalItem;

        public XmlClipboard()
        {
            PasteCommand = new(_ => Choose(SelectedItem), this, nameof(SelectedItem), (x) => x is not null);
            SetPropertyDependency(nameof(Items), nameof(NativeItems), nameof(ExternalItem));
        }

        public event Action OnSelect;

        public IEnumerable<Named<XElement>> Items => ExternalItem is null ? NativeItems : new[] { ExternalItem }.Concat(NativeItems);

        public ObservableCollection<Named<XElement>> NativeItems { get; } = new();

        public XElement ChosenValue { get; private set; }

        public Named<XElement> ExternalItem { get => externalItem; set => SetProperty(ref externalItem, value); }

        public ButtonCommand PasteCommand { get; }

        public Named<XElement> SelectedItem { get => selectedItem; set => SetProperty(ref selectedItem, value); }

        public void SetExternalItem(string str)
        {
            ExternalItem = GetExternalItem();

            Named<XElement> GetExternalItem()
            {
                if (!string.IsNullOrWhiteSpace(str))
                {
                    XElement xml;

                    try
                    {
                        xml = XElement.Parse(str);
                        return new(str, xml);
                    }
                    catch (Exception ex)
                    {
                        this.Log(ex.Message);
                    }
                }

                return null;
            }
        }

        public void Choose(Named<XElement> item)
        {
            if (item is null)
            {
                ChosenValue = null;
                return;
            }

            if (NativeItems.Remove(item) || ExternalItem == item)
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