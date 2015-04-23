
namespace TestLibrary.MockedServer {
    using System;
    using System.Collections.Generic;

    using DotCMIS.Client;

    using Moq;

    public class MockedItemList<T> : Mock<IItemEnumerable<T>> {
        private List<T> internalList;

        public MockedItemList(params T[] entries) : base(MockBehavior.Strict) {
            this.internalList = new List<T>(entries);
            this.ItemsPerPage = this.ItemsPerPage == 0 ? 100 : this.ItemsPerPage;
            this.Setup(l => l.TotalNumItems).Returns(this.internalList.Count);
            this.Setup(l => l.GetEnumerator()).Returns(this.internalList.GetEnumerator());
            this.Setup(l => l.GetPage()).Returns(new MockedItemList<T>(this.internalList.GetRange(0, this.ItemsPerPage).ToArray()) { ItemsPerPage = this.ItemsPerPage }.Object);
            this.Setup(l => l.GetPage(It.IsAny<int>())).Returns<int>((max) => new MockedItemList<T>(this.internalList.GetRange(0, max).ToArray()) { ItemsPerPage = max }.Object);
            this.Setup(l => l.PageNumItems).Returns(this.internalList.Count % this.ItemsPerPage);
            this.Setup(l => l.HasMoreItems).Returns(this.internalList.Count > this.ItemsPerPage);
            this.Setup(l => l.SkipTo(It.Is<int>(i => i > 0))).Returns<int>((pos) => new MockedItemList<T>(this.internalList.GetRange(pos, this.internalList.Count - pos).ToArray()) { ItemsPerPage = this.ItemsPerPage }.Object);
        }

        public int ItemsPerPage { get; set; }
    }
}