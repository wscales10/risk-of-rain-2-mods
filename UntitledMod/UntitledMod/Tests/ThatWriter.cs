using Moq;
using RoR2;
using System.Collections.Generic;
using System.Linq;

namespace UntitledMod.Tests
{
    internal class ThatWriter : TestClass
    {
        public void RefreshItemWeightMultiplier_RemovesAvailableItems()
        {
            var inventoryManagerMock = new Mock<IInventoryManager>();
            inventoryManagerMock.Setup(x => x.IsAllowed(It.IsAny<ItemIndex>())).Returns(true);

            Mock<IInventoryManagers> inventoryManagersMock = new Mock<IInventoryManagers>();
            IEnumerable<IInventoryManager> inventoryManagers = Enumerable.Repeat(inventoryManagerMock.Object, 5).ToArray();
            inventoryManagersMock.Setup(x => x.GetEnumerator()).Returns(inventoryManagers.GetEnumerator());

            var pickupWeightMultipliersMock = new Mock<IPickupWeightMultipliers>();
            var writer = new Writer(this.Logger, inventoryManagersMock.Object, null, pickupWeightMultipliersMock.Object, this.FindPickupIndex);

            var itemIndex = (ItemIndex)69;
            writer.RefreshItemWeightMultiplier(itemIndex);

            var pickupIndex = this.FindPickupIndex(itemIndex);
            pickupWeightMultipliersMock.Verify(x => x.SetValue(It.Is<PickupIndex>(i => i == pickupIndex), It.Is<float?>(v => v == null)), Times.Once);
        }

        public void RefreshItemWeightMultiplier_BansUnavailableItems()
        {
            var itemIndex = (ItemIndex)69;
            var pickupIndex = this.FindPickupIndex(itemIndex);

            var inventoryManagerMock = new Mock<IInventoryManager>();
            inventoryManagerMock.Setup(x => x.IsAllowed(It.IsAny<ItemIndex>())).Returns<ItemIndex>(i => i != itemIndex);

            Mock<IInventoryManagers> inventoryManagersMock = new Mock<IInventoryManagers>();
            IEnumerable<IInventoryManager> inventoryManagers = Enumerable.Repeat(inventoryManagerMock.Object, 5).ToArray();
            inventoryManagersMock.Setup(x => x.GetEnumerator()).Returns(inventoryManagers.GetEnumerator());

            var pickupWeightMultipliersMock = new Mock<IPickupWeightMultipliers>();
            var writer = new Writer(this.Logger, inventoryManagersMock.Object, null, pickupWeightMultipliersMock.Object, this.FindPickupIndex);

            writer.RefreshItemWeightMultiplier(itemIndex);

            pickupWeightMultipliersMock.Verify(x => x.SetValue(It.Is<PickupIndex>(i => i == pickupIndex), It.Is<float?>(v => v == 0)), Times.Once);
        }

        private PickupIndex FindPickupIndex(ItemIndex index) => new PickupIndex((int)index);
    }
}