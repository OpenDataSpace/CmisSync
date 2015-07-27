//-----------------------------------------------------------------------
// <copyright file="RepositoryStatusAggregatorTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.CmisTests {
    using System;
    using System.IO;

    using CmisSync.Lib;
    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.Events;
    using CmisSync.Lib.Queueing;

    using Moq;

    using NUnit.Framework;

    using TestLibrary.IntegrationTests;

    [TestFixture, Category("Fast")]
    public class RepositoryStatusAggregatorTest {
        [Test]
        public void Constructor() {
            var underTest = new RepositoryStatusAggregator();
            Assert.That(underTest.Status, Is.EqualTo(SyncStatus.Disconnected));
            Assert.That(underTest.NumberOfChanges, Is.EqualTo(0));
            Assert.That(underTest.LastFinishedSync, Is.Null);
        }

        [Test]
        public void AddRepository() {
            var underTest = new RepositoryStatusAggregator();
            var numberOfChanges = 2;
            var status = SyncStatus.Idle;
            var lastSync = DateTime.Now;

            underTest.Add(Mock.Of<INotifyRepositoryPropertyChanged>(r => r.NumberOfChanges == numberOfChanges && r.Status == status && r.LastFinishedSync == lastSync));

            Assert.That(underTest.NumberOfChanges, Is.EqualTo(numberOfChanges));
            Assert.That(underTest.LastFinishedSync, Is.EqualTo(lastSync));
            Assert.That(underTest.Status, Is.EqualTo(status));
        }

        [Test]
        public void RemoveRepository() {
            var underTest = new RepositoryStatusAggregator();

            var repo = Mock.Of<INotifyRepositoryPropertyChanged>(r => r.NumberOfChanges == 2 && r.Status == SyncStatus.Idle && r.LastFinishedSync == DateTime.Now);
            underTest.Add(repo);
            underTest.Remove(repo);

            Assert.That(underTest.NumberOfChanges, Is.EqualTo(0));
            Assert.That(underTest.LastFinishedSync, Is.EqualTo(null));
            Assert.That(underTest.Status, Is.EqualTo(SyncStatus.Disconnected));
        }

        [Test]
        public void ThreeRepositoriesAddedAndAggregated() {
            var underTest = new RepositoryStatusAggregator();

            underTest.Add(Mock.Of<INotifyRepositoryPropertyChanged>(r => r.NumberOfChanges == 1 && r.Status == SyncStatus.Idle && r.LastFinishedSync == (DateTime?)null));
            underTest.Add(Mock.Of<INotifyRepositoryPropertyChanged>(r => r.NumberOfChanges == 2 && r.Status == SyncStatus.Disconnected && r.LastFinishedSync == DateTime.Now));
            underTest.Add(Mock.Of<INotifyRepositoryPropertyChanged>(r => r.NumberOfChanges == 0 && r.Status == SyncStatus.Warning && r.LastFinishedSync == DateTime.Now));

            Assert.That(underTest.NumberOfChanges, Is.EqualTo(3));
            Assert.That(underTest.LastFinishedSync, Is.Null);
            Assert.That(underTest.Status, Is.EqualTo(SyncStatus.Warning));
        }
    }
}