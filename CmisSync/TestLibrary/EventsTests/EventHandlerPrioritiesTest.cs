//-----------------------------------------------------------------------
// <copyright file="EventHandlerPrioritiesTest.cs" company="GRAU DATA AG">
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
namespace TestLibrary.EventsTests
{
    using System;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.Producer.ContentChange;
    using CmisSync.Lib.Producer.Crawler;
    using CmisSync.Lib.Producer.Watcher;

    using log4net;
    using log4net.Config;

    using NUnit.Framework;

    [TestFixture]
    public class EventHandlerPrioritiesTest
    {
        [Test, Category("Fast")]
        public void DebugTest()
        {
            int prio = EventHandlerPriorities.GetPriority(typeof(DebugLoggingHandler));
            Assert.That(prio, Is.EqualTo(new DebugLoggingHandler().Priority));
            Assert.That(prio, Is.EqualTo(100000));
        }

        [Test, Category("Fast")]
        public void ContentChangeHigherThanCrawler()
        {
            int contentChange = EventHandlerPriorities.GetPriority(typeof(ContentChanges));
            int crawler = EventHandlerPriorities.GetPriority(typeof(DescendantsCrawler));
            Assert.That(contentChange, Is.GreaterThan(crawler));
        }

        [Test, Category("Fast")]
        public void ContentChangeAccHigherThanTransformer()
        {
            int higher = EventHandlerPriorities.GetPriority(typeof(ContentChangeEventAccumulator));
            int lower = EventHandlerPriorities.GetPriority(typeof(ContentChangeEventTransformer));
            Assert.That(higher, Is.GreaterThan(lower));
        }
    }
}
