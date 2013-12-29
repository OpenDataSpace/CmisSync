using log4net;
using log4net.Config;

using System;
using System.IO;
namespace TestLibrary.EventsTests
{
    using NUnit.Framework;
    using Moq;
    using CmisSync.Lib;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Cmis;
    using DotCMIS.Client;

    [TestFixture]
    public class GenericSyncEventHandlerTest
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GenericSyncEventHandlerTest));

        [TestFixtureSetUp]
        public void ClassInit()
        {
            log4net.Config.XmlConfigurator.Configure(ConfigManager.CurrentConfig.GetLog4NetConfig());
        }


        [Test]
        [Ignore]
        public void ToStringTest() {
            
        }
        
        [Test]
        [Ignore]
        public void PriorityTest() {
            var handler = new FSDeletionHandler(new Mock<IDatabase>().Object, new Mock<ISession>().Object);
            Assert.AreEqual(100, handler.Priority);
            
        }
        
        [Test]
        [Ignore]
        public void IgnoresExpectedEvents() {
            var handler = new FSDeletionHandler(new Mock<IDatabase>().Object, new Mock<ISession>().Object);
            bool handled = handler.Handle(new Mock<ISyncEvent>().Object);
            Assert.False(handled);            
        }

        [Test]
        [Ignore]
        public void IgnoresNonDeleteEvent() {
            var handler = new FSDeletionHandler(new Mock<IDatabase>().Object, new Mock<ISession>().Object);
            bool handled = handler.Handle(new Mock<FSEvent>(WatcherChangeTypes.Created, "").Object);
            Assert.False(handled);            
        }

        private bool EventThrown(ISyncEvent e)
        {
            return false;
        }
    }
}
