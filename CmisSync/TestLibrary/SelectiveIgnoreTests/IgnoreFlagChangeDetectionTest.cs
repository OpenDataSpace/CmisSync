//-----------------------------------------------------------------------
// <copyright file="IgnoreFlagChangeDetectionTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.SelectiveIgnoreTests
{
    using System;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.Queueing;
    using CmisSync.Lib.SelectiveIgnore;

    using DotCMIS.Client;
    using DotCMIS.Enums;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.TestUtils;

    [TestFixture]
    public class IgnoreFlagChangeDetectionTest
    {
        private Mock<IIgnoredEntitiesStorage> ignoreStorage;
        private Mock<IPathMatcher> matcher;
        private Mock<ISyncEventQueue> queue;
        private Mock<ISession> session;

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void DefaultConstructor() {
            this.SetUpMocks();
            new IgnoreFlagChangeDetection(this.ignoreStorage.Object, this.matcher.Object, this.queue.Object);
            this.queue.VerifyThatNoEventIsAdded();
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorFailsIfMatcherIsNull() {
            this.SetUpMocks();
            Assert.Throws<ArgumentNullException>(() => new IgnoreFlagChangeDetection(this.ignoreStorage.Object, null, this.queue.Object));
            this.queue.VerifyThatNoEventIsAdded();
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void ConstructorFailsIfStorageIsNull() {
            this.SetUpMocks();
            Assert.Throws<ArgumentNullException>(() => new IgnoreFlagChangeDetection(null, this.matcher.Object, this.queue.Object));
            this.queue.VerifyThatNoEventIsAdded();
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void DetectionIgnoresNonContentChangeEvents() {
            this.SetUpMocks();
            var underTest = new IgnoreFlagChangeDetection(this.ignoreStorage.Object, this.matcher.Object, this.queue.Object);
            Assert.That(underTest.Handle(Mock.Of<ISyncEvent>()), Is.False);
        }

        [Test, Category("Fast"), Category("SelectiveIgnore")]
        public void CreatedEventForIgnoredObject() {
            this.SetUpMocks();
            var underTest = new IgnoreFlagChangeDetection(this.ignoreStorage.Object, this.matcher.Object, this.queue.Object);
            var createdObject = MockOfIFolderUtil.CreateRemoteFolderMock("id", "name", "/name", "parentId");
            var createdEvent = new ContentChangeEvent(ChangeType.Created, createdObject.Object.Id);
            this.session.Setup(s => s.GetObject(It.IsAny<string>(), It.IsAny<IOperationContext>())).Returns(createdObject.Object);
            createdEvent.UpdateObject(this.session.Object);

            Assert.That(underTest.Handle(createdEvent), Is.False);
        }

        private void SetUpMocks() {
            this.ignoreStorage = new Mock<IIgnoredEntitiesStorage>();
            this.matcher = new Mock<IPathMatcher>();
            this.queue = new Mock<ISyncEventQueue>();
            this.session = new Mock<ISession>();
        }
    }
}