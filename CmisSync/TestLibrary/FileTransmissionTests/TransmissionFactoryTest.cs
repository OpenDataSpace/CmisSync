using System.ComponentModel;
using CmisSync.Lib;


namespace TestLibrary.FileTransmissionTests {
    using System;

    using CmisSync.Lib.Cmis;
    using CmisSync.Lib.Config;
    using CmisSync.Lib.FileTransmission;

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
            if (type == TransmissionType.DOWNLOAD_MODIFIED_FILE || type == TransmissionType.DOWNLOAD_NEW_FILE) {
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

            if (type == TransmissionType.DOWNLOAD_MODIFIED_FILE || type == TransmissionType.DOWNLOAD_NEW_FILE) {
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
            transmission.Status = TransmissionStatus.FINISHED;
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