//-----------------------------------------------------------------------
// <copyright file="ObservableHandlerTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.TestUtils
{
    using CmisSync.Lib.Events;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ObservableHandlerTest {
        [Test, Category("Fast")]
        public void TestFetch() {
            var handler = new ObservableHandler();
            var event1 = new Mock<ISyncEvent>().Object;
            var event2 = new Mock<ISyncEvent>().Object;
            Assert.That(handler.Handle(event1), Is.True);
            handler.Handle(event2);
            Assert.That(handler.List[0], Is.EqualTo(event1));
            Assert.That(handler.List[1], Is.EqualTo(event2));
        }
    }
}
