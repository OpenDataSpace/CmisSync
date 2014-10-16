
namespace TestLibrary.SelectiveIgnoreTests
{
    using System;
    using System.Collections.ObjectModel;

    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.SelectiveIgnore;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture]
    public class FilterTest
    {
        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorFailsIfQueueIsNull() {
            Assert.Throws<ArgumentNullException>(
                () => new SelectiveIgnoreFilter(
                null,
                new ObservableCollection<IIgnoredEntity>(),
                Mock.Of<ISession>()));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorFailsIfCollectionIsNull() {
            Assert.Throws<ArgumentNullException>(
                () => new SelectiveIgnoreFilter(
                Mock.Of<ISyncEventQueue>(),
                null,
                Mock.Of<ISession>()));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorFailsIfSessionIsNull() {
            Assert.Throws<ArgumentNullException>(
                () => new SelectiveIgnoreFilter(
                Mock.Of<ISyncEventQueue>(),
                new ObservableCollection<IIgnoredEntity>(),
                null));
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorTest() {
            new SelectiveIgnoreFilter(
                Mock.Of<ISyncEventQueue>(),
                new ObservableCollection<IIgnoredEntity>(),
                Mock.Of<ISession>());
        }
    }
}