//-----------------------------------------------------------------------
// <copyright file="MockedItemList.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------

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