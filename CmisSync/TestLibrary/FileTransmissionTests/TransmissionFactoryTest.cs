//-----------------------------------------------------------------------
// <copyright file="TransmissionFactoryTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.FileTransmissionTests {
    using System;

    using CmisSync.Lib;
    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.FileTransmission;

    using DataSpace.Common.Transmissions;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class TransmissionFactoryTest {
        [Test, Category("Fast")]
        public void ConstructorFailesIfRepoIsNull() {
            Assert.Throws<ArgumentNullException>(() => new TransmissionFactory(null, Mock.Of<ITransmissionAggregator>()));
        }

        [Test, Category("Fast")]
        public void ConstructorFailesIfAggregatorIsNull() {
            var repo = new Mock<AbstractNotifyingRepository>(new RepoInfo()).Object;
            Assert.Throws<ArgumentNullException>(() => new TransmissionFactory(repo, null));
        }

        [Test, Category("Fast"), TestCaseSource("GetAllTypes")]
        public void FactoryCreatesCorrectNewTransmission(TransmissionType type) {
            string repoName = Guid.NewGuid().ToString();
            long uploadLimit = 1024;
            long downloadLimit = 2048;
            string path = "path";
            var repo = new Mock<AbstractNotifyingRepository>(new RepoInfo() {
                DisplayName = repoName,
                UploadLimit = uploadLimit,
                DownloadLimit = downloadLimit }) {CallBase = true }.Object;
            var aggregator = new Mock<ITransmissionAggregator>();
            var underTest = new TransmissionFactory(repo, aggregator.Object);

            var transmission = underTest.CreateTransmission(type, path);

            Assert.That(transmission.Repository, Is.EqualTo(repoName));
            Assert.That(transmission.Path, Is.EqualTo(path));
            if (type == TransmissionType.DownloadModifiedFile || type == TransmissionType.DownloadNewFile) {
                Assert.That(transmission.MaxBandwidth, Is.EqualTo(downloadLimit));
            } else {
                Assert.That(transmission.MaxBandwidth, Is.EqualTo(uploadLimit));
            }

            aggregator.Verify(a => a.Add(transmission), Times.Once());
        }

        [Test, Category("Fast"), TestCaseSource("GetAllTypes")]
        public void FactoryNotifiesTransmissionAboutBandwidthChanges(TransmissionType type) {
            string repoName = Guid.NewGuid().ToString();
            long uploadLimit = 1024;
            long downloadLimit = 2048;
            var repo = new TestRepository(new RepoInfo() {
                DisplayName = repoName });
            var aggregator = new Mock<ITransmissionAggregator>();
            var underTest = new TransmissionFactory(repo, aggregator.Object);

            var transmission = underTest.CreateTransmission(type, "path");

            repo.Update(new RepoInfo { DisplayName = repoName, DownloadLimit = downloadLimit, UploadLimit = uploadLimit });

            if (type == TransmissionType.DownloadModifiedFile || type == TransmissionType.DownloadNewFile) {
                Assert.That(transmission.MaxBandwidth, Is.EqualTo(downloadLimit));
            } else {
                Assert.That(transmission.MaxBandwidth, Is.EqualTo(uploadLimit));
            }

            aggregator.Verify(a => a.Add(transmission), Times.Once());
        }

        [Test, Category("Fast"), TestCaseSource("GetAllTypes")]
        public void FactoryStopsNotifyFinishedTransmissionAboutBandwidthChanges(TransmissionType type) {
            string repoName = Guid.NewGuid().ToString();
            long uploadLimit = 1024;
            long downloadLimit = 2048;
            var repo = new TestRepository(new RepoInfo() {
                DisplayName = repoName });
            var aggregator = new Mock<ITransmissionAggregator>();
            var underTest = new TransmissionFactory(repo, aggregator.Object);

            var transmission = underTest.CreateTransmission(type, "path");
            transmission.Status = Status.Finished;
            repo.Update(new RepoInfo { DisplayName = repoName, DownloadLimit = downloadLimit, UploadLimit = uploadLimit });

            Assert.That(transmission.MaxBandwidth, Is.EqualTo(0));
            aggregator.Verify(a => a.Add(transmission), Times.Once());
        }

        public Array GetAllTypes(){
            return Enum.GetValues(typeof(TransmissionType));
        }

        private class TestRepository : AbstractNotifyingRepository {
            public TestRepository(RepoInfo info) : base(info) {
            }

            public void Update(RepoInfo newInfo) {
                base.RepoInfo = newInfo;
            }
        }
    }
}