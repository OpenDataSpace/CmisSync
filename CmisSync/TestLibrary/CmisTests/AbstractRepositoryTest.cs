//-----------------------------------------------------------------------
// <copyright file="AbstractRepositoryTest.cs" company="GRAU DATA AG">
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

    using CmisSync.Lib;
    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Config;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class AbstractRepositoryTest {
        private readonly string localPath = "path";
        private readonly string name = "my";
        private readonly Uri remoteUrl = new Uri("https://demo.deutsche-wolke.de/cmis/browser");

        [Test, Category("Fast")]
        public void ConstructorTakesRepoInfo() {
            var underTest = this.CreateRepo();

            Assert.That(underTest.Name, Is.EqualTo(this.name));
            Assert.That(underTest.LocalPath, Is.EqualTo(this.localPath));
            Assert.That(underTest.RemoteUrl, Is.EqualTo(this.remoteUrl));
            Assert.That(underTest.LastFinishedSync, Is.Null);
            Assert.That(underTest.NumberOfChanges, Is.EqualTo(0));
        }

        [Test, Category("Fast")]
        public void ConstructorThrowsExceptionIfRepoInfoIsNull() {
            Assert.Throws<ArgumentNullException>(() => new TestRepository(null));
        }

        [Test, Category("Fast")]
        public void NotificationsOnNumberChanges() {
            var underTest = this.CreateRepo();
            var expectedNumber = 2;
            string propertyName = null;
            underTest.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => {
                Assert.That(sender, Is.EqualTo(underTest));
                Assert.That(e.PropertyName, Is.Not.Null);
                propertyName = e.PropertyName;
            };

            underTest.SetNumberOfChanges(expectedNumber);
            Assert.That(underTest.NumberOfChanges, Is.EqualTo(expectedNumber));
            Assert.That(propertyName, Is.EqualTo(Utils.NameOf((TestRepository t) => t.NumberOfChanges)));
        }

        [Test, Category("Fast")]
        public void NotificationsOnDateChanges() {
            var underTest = this.CreateRepo();
            var expectedDate = DateTime.UtcNow;
            string propertyName = null;
            underTest.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) => {
                Assert.That(sender, Is.EqualTo(underTest));
                Assert.That(e.PropertyName, Is.Not.Null);
                propertyName = e.PropertyName;
            };

            underTest.SetLastFinishedSync(expectedDate);
            Assert.That(underTest.LastFinishedSync, Is.EqualTo(expectedDate));
            Assert.That(propertyName, Is.EqualTo(Utils.NameOf((TestRepository t) => t.LastFinishedSync)));
        }

        private TestRepository CreateRepo() {
            var info = new RepoInfo {
                LocalPath = this.localPath,
                Address = this.remoteUrl,
                DisplayName = this.name
            };
            return new TestRepository(info);
        }

        private class TestRepository : AbstractNotifyingRepository {
            public TestRepository(RepoInfo info) : base(info) {
            }

            public void SetNumberOfChanges(int number) {
                this.NumberOfChanges = number;
            }

            public void SetLastFinishedSync(DateTime? date) {
                this.LastFinishedSync = date;
            }
        }
    }
}