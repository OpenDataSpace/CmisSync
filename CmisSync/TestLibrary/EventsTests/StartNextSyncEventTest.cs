using System;

using CmisSync.Lib.Events;

using NUnit.Framework;

using Moq;
namespace TestLibrary.EventsTests
{
    [TestFixture]
    public class StartNextSyncEventTest
    {
        [Test, Category("Fast")]
        public void ContructorWithoutParamTest() {
            var start = new StartNextSyncEvent();
            Assert.IsFalse(start.FullSyncRequested);
        }

        [Test, Category("Fast")]
        public void ConstructorWithFalseParamTest() {
            var start = new StartNextSyncEvent(false);
            Assert.IsFalse(start.FullSyncRequested);
        }

        [Test, Category("Fast")]
        public void ConstructorWithTrueParamTest() {
            var start = new StartNextSyncEvent(true);
            Assert.IsTrue(start.FullSyncRequested);
        }

        [Test, Category("Fast")]
        public void ParamTest() {
            var start = new StartNextSyncEvent();
            string key = "key";
            string value = "value";

            start.SetParam(key, value);
            string result;
            Assert.IsTrue(start.TryGetParam(key, out result));
            Assert.AreEqual(value, result);
            Assert.IsFalse(start.TryGetParam("k", out result));
        }
    }
}

