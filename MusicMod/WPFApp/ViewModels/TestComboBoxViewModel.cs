using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WPFApp.ViewModels
{
    public class Item : NotifyPropertyChangedBase
    {
        private string name;

        public Item(string name) => Name = name;

        public string Name { get => name; set => SetProperty(ref name, value); }

        public static implicit operator Item(string name) => new(name);
    }

    public class TestComboBoxViewModel : ViewModelBase
    {
        private Item selectedItem;

        private string text;

        public TestComboBoxViewModel()
        {
            Items = new(new[] { A, B });
            Items.CollectionChanged += Items_CollectionChanged;
            AddCommand = new ButtonCommand(_ => Items.Add(B));
            RemoveCommand = new ButtonCommand(_ => Items.Remove(B));
        }

        public Item A { get; } = "a";

        public Item B { get; } = "b";

        public ObservableCollection<Item> Items { get; }

        public ICommand AddCommand { get; }

        public ICommand RemoveCommand { get; }

        public string Text
        {
            get => text;

            set => SetProperty(ref text, value);
        }

        public Item SelectedItem
        {
            get => selectedItem;

            set
            {
                if (selectedItem is not null)
                {
                    selectedItem.PropertyChanged -= Value_PropertyChanged;
                }

                if (value is not null)
                {
                    value.PropertyChanged += Value_PropertyChanged;
                }

                SetProperty(ref selectedItem, value);
            }
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is not null && SelectedItem is null)
            {
                SelectedItem = Items.FirstOrDefault(i => i.Name == Text);
            }
        }

        private void Value_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Item.Name))
            {
                Text = ((Item)sender).Name;
            }
        }
    }
}