//-----------------------------------------------------------------------
// <copyright file="SyncEventQueueTest.cs" company="GRAU DATA AG">
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
    using System.Threading;
    using System.Threading.Tasks;

    using CmisSync.Lib.Config;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;

    using log4net;
    using log4net.Config;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class SyncEventQueueTest : IsTestWithConfiguredLog4Net {
        [Test, Category("Fast")]
        public void EventlessStartStop() {
            using (SyncEventQueue queue = new SyncEventQueue(new Mock<ISyncEventManager>().Object)) {
                WaitFor(queue, (q) => { return !q.IsStopped; });
                Assert.False(queue.IsStopped);
                queue.StopListener();
                WaitFor(queue, (q) => { return q.IsStopped; });
                Assert.True(queue.IsStopped);
            }
        }

        [Test, Category("Fast")]
        public void AddEvent() {
            var managerMock = new Mock<ISyncEventManager>();
            var eventMock = new Mock<ISyncEvent>();
            using (SyncEventQueue queue = new SyncEventQueue(managerMock.Object)) {
                queue.AddEvent(eventMock.Object);
                queue.AddEvent(eventMock.Object);
                queue.StopListener();
                WaitFor(queue, (q) => { return q.IsStopped; });
                Assert.True(queue.IsStopped);
                Assert.True(queue.IsEmpty);
            }

            managerMock.Verify(foo => foo.Handle(eventMock.Object), Times.Exactly(2));
        }

        [Test, Category("Fast")]
        public void AddEventToStoppedQueueDoesNotRaise() {
            using (SyncEventQueue queue = new SyncEventQueue(new Mock<ISyncEventManager>().Object)) {
                queue.StopListener();
                WaitFor(queue, (q) => { return q.IsStopped; });
                queue.AddEvent(new Mock<ISyncEvent>().Object);
            }
        }

        [Test, Category("Fast")]
        public void AddEventToDisposedQueueDoesNotRaise() {
            SyncEventQueue queue = new SyncEventQueue(new Mock<ISyncEventManager>().Object);
            queue.Dispose();

            queue.AddEvent(new Mock<ISyncEvent>().Object);
        }

        [Test, Category("Medium")]
        public void WaitForStop() {
            Task t;
            using (SyncEventQueue queue = new SyncEventQueue(new Mock<ISyncEventManager>().Object)) {
                t = Task.Factory.StartNew(() => { Thread.Sleep(100); queue.StopListener(); });
                queue.WaitForStopped();
                Assert.True(queue.IsStopped);
            }

            t.Wait();
        }

        [Test, Category("Medium")]
        public void WaitForStopWithTimeout() {
            Task t;
            using (SyncEventQueue queue = new SyncEventQueue(new Mock<ISyncEventManager>().Object)) {
                t = Task.Factory.StartNew(() => { Thread.Sleep(100); queue.StopListener(); });
                Assert.False(queue.WaitForStopped(10));
                Assert.True(queue.WaitForStopped(1000));
                Assert.True(queue.IsStopped);
            }

            t.Wait();
        }

        [Test, Category("Medium")]
        public void WaitForStopWithTimeSpan() {
            Task t;
            using (SyncEventQueue queue = new SyncEventQueue(new Mock<ISyncEventManager>().Object)) {
                t = Task.Factory.StartNew(() => { Thread.Sleep(100); queue.StopListener(); });
                Assert.False(queue.WaitForStopped(new TimeSpan(0, 0, 0, 0, 10)));
                Assert.True(queue.WaitForStopped(new TimeSpan(0, 0, 0, 0, 1000)));
                Assert.True(queue.IsStopped);
            }

            t.Wait();
        }

        [Test, Category("Fast")]
        public void ExceptionsInManagerAreHandled() {
            var managerMock = new Mock<ISyncEventManager>();
            managerMock.Setup(m => m.Handle(It.IsAny<ISyncEvent>())).Throws(new Exception("Generic Exception Message"));
            var eventMock = new Mock<ISyncEvent>();
            eventMock.Setup(e => e.ToString()).Returns("Mocked Event");
            using (SyncEventQueue queue = new SyncEventQueue(managerMock.Object)) {
                queue.AddEvent(eventMock.Object);
                queue.StopListener();
                WaitFor(queue, (q) => { return q.IsStopped; });
                Assert.True(queue.IsStopped);
            }
        }

        [Test, Category("Fast")]
        public void SubscribeForAllCountableEvents() {
            using (SyncEventQueue queue = new SyncEventQueue(Mock.Of<ISyncEventManager>())) {
                using (var unsubscriber = queue.FullCounter.Subscribe(Mock.Of<IObserver<int>>())) {
                    Assert.That(unsubscriber, Is.Not.Null);
                }
            }
        }

        [Test, Category("Fast")]
        public void SubscribeForAllCategorizesCountableEvents() {
            using (SyncEventQueue queue = new SyncEventQueue(Mock.Of<ISyncEventManager>())) {
                using (var unsubscriber = queue.CategoryCounter.Subscribe(Mock.Of<IObserver<Tuple<string, int>>>())) {
                    Assert.That(unsubscriber, Is.Not.Null);
                }
            }
        }

        [Test, Category("Fast")]
        public void SubscribeThrowsExceptionIfAllObserverIsNull() {
            using (SyncEventQueue queue = new SyncEventQueue(Mock.Of<ISyncEventManager>())) {
                Assert.Throws<ArgumentNullException>(
                    () => {
                    using (var unsubscriber = queue.FullCounter.Subscribe((IObserver<int>)null)) {
                    }
                });
            }
        }

        [Test, Category("Fast")]
        public void SubscribeThrowsExceptionIfCategorizedObserverIsNull() {
            using (SyncEventQueue queue = new SyncEventQueue(Mock.Of<ISyncEventManager>())) {
                Assert.Throws<ArgumentNullException>(
                    () => {
                    using (var unsubscriber = queue.CategoryCounter.Subscribe((IObserver<Tuple<string, int>>)null)) {
                    }
                });
            }
        }

        [Test, Category("Medium")]
        public void SubscribeForAllCountableEventsAndGetInformedOnAddEvent([Values(1, 5, 1000)]int events) {
            string category = "test";
            int lastCount = -1;
            var manager = new Mock<ISyncEventManager>();
            manager.Setup(m => m.Handle(It.IsAny<ICountableEvent>())).Callback(() => Thread.Sleep(10));
            var observer = new Mock<IObserver<int>>();
            observer.Setup(o => o.OnNext(It.IsAny<int>())).Callback<int>(t => { lastCount = t; Assert.That(lastCount, Is.LessThanOrEqualTo(events).And.AtLeast(0));});
            using (SyncEventQueue queue = new SyncEventQueue(manager.Object)) {
                using (var unsubscriber = queue.FullCounter.Subscribe(observer.Object)) {
                    for (int i = 0; i < events; i++) {
                        queue.AddEvent(Mock.Of<ICountableEvent>(e => e.Category == category));
                    }

                    queue.StopListener();
                    WaitFor(queue, (q) => { return q.IsStopped; }, events * 20 + 5000);
                    queue.Dispose();
                }

                observer.Verify(o => o.OnNext(It.IsAny<int>()), Times.Exactly(2 * events));
                Assert.That(lastCount, Is.EqualTo(0));
            }

            observer.Verify(o => o.OnCompleted(), Times.Once());
            manager.Verify(m => m.Handle(It.Is<ICountableEvent>(e => e.Category == category)), Times.Exactly(events));
        }

        [Test, Category("Medium")]
        public void SubscribeForCategoryCountableEventsAndGetInformedOnAddEvent([Values(1, 5, 1000)]int events) {
            string category = "test";
            int lastCount = -1;
            var manager = new Mock<ISyncEventManager>();
            manager.Setup(m => m.Handle(It.IsAny<ICountableEvent>())).Callback(() => Thread.Sleep(10));
            var observer = new Mock<IObserver<Tuple<string, int>>>();
            observer.Setup(o => o.OnNext(It.IsAny<Tuple<string, int>>())).Callback<Tuple<string, int>>(t => { lastCount = t.Item2; Assert.That(lastCount, Is.LessThanOrEqualTo(events).And.AtLeast(0));});
            using (SyncEventQueue queue = new SyncEventQueue(manager.Object)) {
                using (var unsubscriber = queue.CategoryCounter.Subscribe(observer.Object)) {
                    for (int i = 0; i < events; i++) {
                        queue.AddEvent(Mock.Of<ICountableEvent>(e => e.Category == category));
                    }

                    queue.StopListener();
                    WaitFor(queue, (q) => { return q.IsStopped; }, events * 20 + 5000);
                    queue.Dispose();
                }

                observer.Verify(o => o.OnNext(It.Is<Tuple<string, int>>(t => t.Item1 == category)), Times.Exactly(2 * events));
                Assert.That(lastCount, Is.EqualTo(0));
            }

            observer.Verify(o => o.OnCompleted(), Times.Once());
            manager.Verify(m => m.Handle(It.Is<ICountableEvent>(e => e.Category == category)), Times.Exactly(events));
        }

        [Test, Category("Medium")]
        public void ExceptionRaisingEventsDecreaseFullEventCounter([Values(1, 5, 10)]int events) {
            string category = "test";
            int lastCount = -1;
            var manager = new Mock<ISyncEventManager>();
            manager.Setup(m => m.Handle(It.IsAny<ICountableEvent>())).Callback(() => Thread.Sleep(10)).Throws(new Exception("Generic Exception"));
            var observer = new Mock<IObserver<int>>();
            observer.Setup(o => o.OnNext(It.IsAny<int>())).Callback<int>(t => { lastCount = t; Assert.That(lastCount, Is.LessThanOrEqualTo(events).And.AtLeast(0));});
            using (SyncEventQueue queue = new SyncEventQueue(manager.Object)) {
                using (var unsubscriber = queue.FullCounter.Subscribe(observer.Object)) {
                    for (int i = 0; i < events; i++) {
                        queue.AddEvent(Mock.Of<ICountableEvent>(e => e.Category == category));
                    }

                    queue.StopListener();
                    WaitFor(queue, (q) => { return q.IsStopped; }, events * 20 + 5000);
                    queue.Dispose();
                }

                observer.Verify(o => o.OnNext(It.IsAny<int>()), Times.Exactly(2 * events));
                Assert.That(lastCount, Is.EqualTo(0));
            }

            observer.Verify(o => o.OnCompleted(), Times.Once());
            manager.Verify(m => m.Handle(It.Is<ICountableEvent>(e => e.Category == category)), Times.Exactly(events));
        }

        [Test, Category("Medium")]
        public void ExceptionRaisingEventsDecreaseCategoryEventCounter([Values(1, 5, 10)]int events) {
            string category = "test";
            int lastCount = -1;
            var manager = new Mock<ISyncEventManager>();
            manager.Setup(m => m.Handle(It.IsAny<ICountableEvent>())).Callback(() => Thread.Sleep(10)).Throws(new Exception("Generic Exception"));
            var observer = new Mock<IObserver<Tuple<string, int>>>();
            observer.Setup(o => o.OnNext(It.IsAny<Tuple<string, int>>())).Callback<Tuple<string, int>>(t => { lastCount = t.Item2; Assert.That(lastCount, Is.LessThanOrEqualTo(events).And.AtLeast(0));});
            using (SyncEventQueue queue = new SyncEventQueue(manager.Object)) {
                using (var unsubscriber = queue.CategoryCounter.Subscribe(observer.Object)) {
                    for (int i = 0; i < events; i++) {
                        queue.AddEvent(Mock.Of<ICountableEvent>(e => e.Category == category));
                    }

                    queue.StopListener();
                    WaitFor(queue, (q) => { return q.IsStopped; }, events * 20 + 5000);
                    queue.Dispose();
                }

                observer.Verify(o => o.OnNext(It.Is<Tuple<string, int>>(t => t.Item1 == category)), Times.Exactly(2 * events));
                Assert.That(lastCount, Is.EqualTo(0));
            }

            observer.Verify(o => o.OnCompleted(), Times.Once());
            manager.Verify(m => m.Handle(It.Is<ICountableEvent>(e => e.Category == category)), Times.Exactly(events));
        }

        private static void WaitFor<T>(T obj, Func<T, bool> check, int timeout = 5000) {
            for (int i = 0; i < (int)(timeout/100); i++) {
                if (check(obj)) {
                    return;
                }

                Thread.Sleep(100);
            }

            Assert.Fail("Timeout exceeded!");
        }
    }
}