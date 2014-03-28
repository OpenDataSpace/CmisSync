using log4net;
using log4net.Config;

using System;
using System.Threading.Tasks;
using System.Threading;
namespace TestLibrary.EventsTests
{
    using NUnit.Framework;
    using Moq;
    using CmisSync.Lib;
    using CmisSync.Lib.Events;

    [TestFixture]
    public class SyncEventQueueTest
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SyncEventQueueTest));

        [TestFixtureSetUp]
        public void ClassInit()
        {
            log4net.Config.XmlConfigurator.Configure(ConfigManager.CurrentConfig.GetLog4NetConfig());
        }

        private static void WaitFor<T>(T obj, Func<T,bool> check){
            for(int i = 0; i < 50; i++){
                if (check(obj)) {
                    return;
                }
                Thread.Sleep(100);
            }
            Logger.Error("Timeout exceeded!");
        }

        [Test, Category("Fast")]
        public void EventlessStartStop() {
            using(SyncEventQueue queue = new SyncEventQueue(new Mock<SyncEventManager>().Object)){
                WaitFor(queue, (q) => { return !q.IsStopped; } );
                Assert.False(queue.IsStopped);
                queue.StopListener();
                WaitFor(queue, (q) => { return q.IsStopped; } );
                Assert.True(queue.IsStopped);
            }
        }

        [Test, Category("Fast")]
        public void AddEvent() {
            var managerMock = new Mock<SyncEventManager>();
            var eventMock = new Mock<ISyncEvent>();
            using(SyncEventQueue queue = new SyncEventQueue(managerMock.Object)){
                queue.AddEvent(eventMock.Object);
                queue.AddEvent(eventMock.Object);
                queue.StopListener();
                WaitFor(queue, (q) => { return q.IsStopped; } );
                Assert.True(queue.IsStopped);
            }
            managerMock.Verify(foo => foo.Handle(eventMock.Object), Times.Exactly(2));
        }

        [Test, Category("Fast")]
        [ExpectedException( typeof( InvalidOperationException ) )]
        public void AddEventToStoppedQueue() {
            using(SyncEventQueue queue = new SyncEventQueue(new Mock<SyncEventManager>().Object)){
                queue.StopListener();
                WaitFor(queue, (q) => { return q.IsStopped; } );
                queue.AddEvent(new Mock<ISyncEvent>().Object);
            }
        }

        [Test, Category("Fast")]
        public void WaitForStop() {
            Task t;
            using(SyncEventQueue queue = new SyncEventQueue(new Mock<SyncEventManager>().Object)) {
                t = Task.Factory.StartNew(()=>{Thread.Sleep(100); queue.StopListener();});
                queue.WaitForStopped();
                Assert.True(queue.IsStopped);
            }
            t.Wait();
        }

        [Test, Category("Fast")]
        public void WaitForStopWithTimeout() {
            Task t;
            using(SyncEventQueue queue = new SyncEventQueue(new Mock<SyncEventManager>().Object)) {
                t = Task.Factory.StartNew(()=>{Thread.Sleep(100); queue.StopListener();});
                Assert.False(queue.WaitForStopped(10));
                Assert.True(queue.WaitForStopped(10000));
                Assert.True(queue.IsStopped);
            }
            t.Wait();
        }

        [Test, Category("Fast")]
        public void WaitForStopWithTimeSpan() {
            Task t;
            using(SyncEventQueue queue = new SyncEventQueue(new Mock<SyncEventManager>().Object)) {
                t = Task.Factory.StartNew(()=>{Thread.Sleep(100); queue.StopListener();});
                Assert.False(queue.WaitForStopped(new TimeSpan(0,0,0,0,10)));
                Assert.True(queue.WaitForStopped(new TimeSpan(0,0,0,0,500)));
                Assert.True(queue.IsStopped);
            }
            t.Wait();
        }
    }
}
