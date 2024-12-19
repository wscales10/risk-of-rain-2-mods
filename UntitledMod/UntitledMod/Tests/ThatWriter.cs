using Castle.Core.Logging;
using Moq;
using Moq.AutoMock;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace UntitledMod.Tests
{
    internal class ThatWriter : TestClass
    {
        protected override AutoMocker GetMocker()
        {
            var mocker = base.GetMocker();
            mocker.Use<Func<ItemIndex, PickupIndex>>(this.FindPickupIndex);
            return mocker;
        }

        public void RefreshItemWeightMultipliers_RemovesAvailableItems()
        {
            var mocker = this.GetMocker();
            mocker.Use((ServerSide)null);

            var inventoryManagerMock = mocker.GetMock<IInventoryManager>();
            inventoryManagerMock.Setup(x => x.IsAllowed(It.IsAny<ItemIndex>())).Returns(true);

            var inventoryManagersMock = mocker.GetMock<IInventoryManagers>();
            IEnumerable<IInventoryManager> inventoryManagers = Enumerable.Repeat(inventoryManagerMock.Object, 5).ToArray();
            inventoryManagersMock.Setup(x => x.GetEnumerator()).Returns(inventoryManagers.GetEnumerator());

            var writer = mocker.CreateInstance<Writer>();

            var itemIndex = (ItemIndex)69;
            writer.RefreshItemWeightMultipliers(itemIndex);

            var pickupIndex = this.FindPickupIndex(itemIndex);
            mocker.GetMock<IPickupWeightMultipliers>().Verify(x => x.SetValue(It.Is<PickupIndex>(i => i == pickupIndex), It.Is<float?>(v => v == null)), Times.Once);
        }

        public void RefreshItemWeightMultipliers_BansUnavailableItems()
        {
            var mocker = this.GetMocker();
            mocker.Use((ServerSide)null);

            var itemIndex = (ItemIndex)69;
            var pickupIndex = this.FindPickupIndex(itemIndex);

            var inventoryManagerMock = mocker.GetMock<IInventoryManager>();
            inventoryManagerMock.Setup(x => x.IsAllowed(It.IsAny<ItemIndex>())).Returns<ItemIndex>(i => i != itemIndex);

            var inventoryManagersMock = mocker.GetMock<IInventoryManagers>();
            IEnumerable<IInventoryManager> inventoryManagers = Enumerable.Repeat(inventoryManagerMock.Object, 5).ToArray();
            inventoryManagersMock.Setup(x => x.GetEnumerator()).Returns(inventoryManagers.GetEnumerator());

            var writer = mocker.CreateInstance<Writer>();

            writer.RefreshItemWeightMultipliers(itemIndex);

            mocker.GetMock<IPickupWeightMultipliers>().Verify(x => x.SetValue(It.Is<PickupIndex>(i => i == pickupIndex), It.Is<float?>(v => v == 0)), Times.Once);
        }

        private PickupIndex FindPickupIndex(ItemIndex index) => new PickupIndex((int)index);
    }
}