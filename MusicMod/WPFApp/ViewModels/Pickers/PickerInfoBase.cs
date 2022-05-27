using System;
using System.Collections;
using System.Collections.Generic;

namespace WPFApp.ViewModels.Pickers
{
    public abstract class PickerInfoBase<T> : IPickerInfo
    {
        protected PickerInfoBase(NavigationContext navigationContext)
        {
            NavigationContext = navigationContext;
        }

        public abstract string DisplayMemberPath { get; }

        public abstract string SelectedValuePath { get; }

        public NavigationContext NavigationContext { get; }

        public abstract object CreateItem(T selectedType);

        public abstract IEnumerable<T> GetItems();

        public DiscreteMenuViewModel GetSelectorViewModel()
        {
            var items = GetItems();

            if (items is ICollection<T> itemsCollection)
            {
                return new DiscreteMenuViewModel<T>(itemsCollection);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        object IPickerInfo.CreateItem(object selectedType) => CreateItem((T)selectedType);

        IEnumerable IPickerInfo.GetItems() => GetItems();
    }
}