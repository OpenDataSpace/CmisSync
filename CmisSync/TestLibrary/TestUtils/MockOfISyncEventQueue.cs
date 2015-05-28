//-----------------------------------------------------------------------
// <copyright file="MockOfISyncEventQueue.cs" company="GRAU DATA AG">
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
    using System;
    using System.Linq.Expressions;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;

    using Moq;

    public static class MockOfISyncEventQueue
    {
        public static void VerifyThatNoOtherEventIsAddedThan<T>(this Mock<ISyncEventQueue> queue) {
            queue.Verify(q => q.AddEvent(It.Is<ISyncEvent>(e => !(e is T))), Times.Never());
        }

        public static void VerifyThatNoEventIsAdded(this Mock<ISyncEventQueue> queue) {
            queue.Verify(q => q.AddEvent(It.IsAny<ISyncEvent>()), Times.Never());
        }
    }
}