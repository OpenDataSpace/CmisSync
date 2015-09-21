//-----------------------------------------------------------------------
// <copyright file="StartNextSyncEventTest.cs" company="GRAU DATA AG">
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
namespace TestLibrary.EventsTests {
    using System;

    using CmisSync.Lib.Events;

    using Moq;

    using NUnit.Framework;

    [TestFixture, Category("Fast")]
    public class StartNextSyncEventTest {
        [Test]
        public void ContructorWithoutParamSetsFullSyncRequestedFlagToFalse() {
            var start = new StartNextSyncEvent();
            Assert.IsFalse(start.FullSyncRequested);
        }

        [Test]
        public void ConstructorWithFalseParamSetsFullSyncRequestedFlagToFalse() {
            var start = new StartNextSyncEvent(false);
            Assert.IsFalse(start.FullSyncRequested);
        }

        [Test]
        public void ConstructorWithTrueParamSetsFullSyncRequestedFlagToTrue() {
            var start = new StartNextSyncEvent(true);
            Assert.IsTrue(start.FullSyncRequested);
        }
    }
}