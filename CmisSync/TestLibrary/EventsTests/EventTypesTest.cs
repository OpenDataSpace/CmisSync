using log4net;
using log4net.Config;

using System;
using System.IO;
namespace TestLibrary.EventsTests
{
    using NUnit.Framework;
    using CmisSync.Lib;
    using CmisSync.Lib.Events;

    [TestFixture]
    public class EventTypesTest
    {
        [Test, Category("Fast")]
        public void FSEventTest() {
            ISyncEvent e = new FSEvent(WatcherChangeTypes.Created, "test");
            Assert.AreEqual("FSEvent with type \"Created\" on path \"test\"", e.ToString());
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void FSEventPreventNullTest() {
            new FSEvent(WatcherChangeTypes.Created,null);
        }
    }
}
