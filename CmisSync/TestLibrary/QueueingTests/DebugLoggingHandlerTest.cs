//-----------------------------------------------------------------------
// <copyright file="DebugLoggingHandlerTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.QueueingTests {
    using System;
    using System.IO;

    using CmisSync.Lib.Config;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;

    using log4net;
    using log4net.Config;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture, Category("Fast")]
    public class DebugLoggingHandlerTest : IsTestWithConfiguredLog4Net {
        [Test]
        public void ToStringTest() {
            var handler = new DebugLoggingHandler();
            Assert.That(handler.ToString().Contains(handler.Priority.ToString()));
            Assert.That(handler.ToString().Contains(handler.GetType().Name));
        }

        [Test]
        public void PriorityTest() {
            var handler = new DebugLoggingHandler();
            Assert.That(handler.Priority, Is.EqualTo(EventHandlerPriorities.DEBUG));
        }
    }
}