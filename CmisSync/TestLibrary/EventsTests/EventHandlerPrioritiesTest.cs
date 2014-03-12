using log4net;
using log4net.Config;

using System;
using System.IO;
namespace TestLibrary.EventsTests
{
    using NUnit.Framework;
    using CmisSync.Lib;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Sync.Strategy;

    [TestFixture]
    public class EventHandlerPrioritiesTest
    {
        [Test, Category("Fast")]
        public void DebugTest() {
            int prio = EventHandlerPriorities.GetPriority(typeof(DebugLoggingHandler));
            Assert.That(prio, Is.EqualTo(new DebugLoggingHandler().Priority));
            Assert.That(prio, Is.EqualTo(100000));
        }


        [Test, Category("Fast")]
        public void ContentChangeHigherThanCrawler() {
            int contentChange = EventHandlerPriorities.GetPriority(typeof(ContentChanges));
            int crawler = EventHandlerPriorities.GetPriority(typeof(Crawler));
            Assert.That(contentChange, Is.GreaterThan(crawler));
        }

        [Test, Category("Fast")]
        public void ContentChangeAccHigherThanTransformer() {
            int higher = EventHandlerPriorities.GetPriority(typeof(ContentChangeEventAccumulator));
            int lower = EventHandlerPriorities.GetPriority(typeof(ContentChangeEventTransformer));
            Assert.That(higher, Is.GreaterThan(lower));
        }
        
        

    }
}
