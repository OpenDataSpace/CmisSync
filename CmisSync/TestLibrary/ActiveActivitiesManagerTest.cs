using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestLibrary
{
    using NUnit.Framework;
    using Moq;
    using CmisSync.Lib;
    using CmisSync.Lib.Events;

    [TestFixture]
    public class ActiveActivitiesManagerTest
    {
        [Test, Category("Fast")]
        public void AddTransmissionTest() {
            ActiveActivitiesManager manager = new ActiveActivitiesManager();
            FileTransmissionType downloadType = FileTransmissionType.DOWNLOAD_NEW_FILE;
            FileTransmissionType downloadModifiedType = FileTransmissionType.DOWNLOAD_MODIFIED_FILE;
            string downloadfile = "Download.txt";
            FileTransmissionEvent downloadTransmission = new FileTransmissionEvent(downloadType, downloadfile);
            Assert.True(manager.AddTransmission(downloadTransmission), "A new file transmission event should be able to be added on the first time, but wasn't");
            Assert.AreEqual(1, manager.ActiveTransmissions.Count);
            Assert.False(manager.AddTransmission(downloadTransmission), "An already existing file transmission event should be rejected on the second insertion, but wasn't");
            Assert.AreEqual(1, manager.ActiveTransmissions.Count);

            downloadTransmission.ReportProgress(new TransmissionProgressEventArgs() { ActualPosition = 0, Length = 1024 });
            Assert.AreEqual(1, manager.ActiveTransmissions.Count);
            Assert.False(manager.AddTransmission(downloadTransmission), "Transmission is not completed, so the addition of the same event must fail, but didn't");
            Assert.AreEqual(1, manager.ActiveTransmissions.Count);
            downloadTransmission.ReportProgress(new TransmissionProgressEventArgs() { BitsPerSecond = 1024 });
            Assert.AreEqual(1, manager.ActiveTransmissions.Count);
            Assert.False(manager.AddTransmission(downloadTransmission), "Transmission is not completed, so the addition of the same event must fail, but didn't");
            Assert.AreEqual(1, manager.ActiveTransmissions.Count);
            downloadTransmission.ReportProgress(new TransmissionProgressEventArgs() { ActualPosition = 1024 });
            Assert.AreEqual(1, manager.ActiveTransmissions.Count);
            Assert.False(manager.AddTransmission(downloadTransmission), "Transmission is not completed, so the addition of the same event must fail, but didn't");
            Assert.AreEqual(1, manager.ActiveTransmissions.Count);
            downloadTransmission.ReportProgress(new TransmissionProgressEventArgs() { Completed = false });
            Assert.AreEqual(1, manager.ActiveTransmissions.Count);
            Assert.False(manager.AddTransmission(downloadTransmission), "Transmission is not completed, so the addition of the same event must fail, but didn't");
            Assert.AreEqual(1, manager.ActiveTransmissions.Count);

            FileTransmissionEvent downloadModifiedTransmission = new FileTransmissionEvent(downloadModifiedType, downloadfile);
            Assert.True(manager.AddTransmission(downloadModifiedTransmission), "A new modified transmission event should be able to be added on the first time, but wasn't");
            Assert.AreEqual(2, manager.ActiveTransmissions.Count);
            Assert.False(manager.AddTransmission(downloadModifiedTransmission), "An already existing file transmission event should be rejected on the second insertion, but wasn't");
            Assert.AreEqual(2, manager.ActiveTransmissions.Count);
            Assert.False(manager.AddTransmission(downloadModifiedTransmission), "An already existing file transmission event should be rejected on the third insertion, but wasn't");
            Assert.AreEqual(2, manager.ActiveTransmissions.Count);
            downloadModifiedTransmission.ReportProgress(new TransmissionProgressEventArgs() { Completed = true });
            Assert.AreEqual(1, manager.ActiveTransmissions.Count);
            Assert.True(manager.AddTransmission(downloadModifiedTransmission), "A completed file transmission event should be able to be readded after, but wasn't");
            Assert.AreEqual(1, manager.ActiveTransmissions.Count);
            downloadModifiedTransmission.ReportProgress(new TransmissionProgressEventArgs() { Completed = true });
            Assert.AreEqual(1, manager.ActiveTransmissions.Count);

            downloadTransmission.ReportProgress(new TransmissionProgressEventArgs() { Aborting = true });
            Assert.AreEqual(1, manager.ActiveTransmissions.Count);
            Assert.False(manager.AddTransmission(downloadTransmission), "An already existing file transmission event should be rejected on the second insertion, but wasn't");
            Assert.AreEqual(1, manager.ActiveTransmissions.Count);
            Assert.False(manager.AddTransmission(downloadTransmission), "An already existing file transmission event should be rejected on the third insertion, but wasn't");
            Assert.AreEqual(1, manager.ActiveTransmissions.Count);
            downloadTransmission.ReportProgress(new TransmissionProgressEventArgs() { Aborted = true });
            Assert.AreEqual(0, manager.ActiveTransmissions.Count);
            Assert.True(manager.AddTransmission(downloadTransmission), "A aborted file transmission event should be able to be readded after, but wasn't");
            Assert.AreEqual(0, manager.ActiveTransmissions.Count);
            downloadTransmission.ReportProgress(new TransmissionProgressEventArgs() { Aborted = true });
            Assert.AreEqual(0, manager.ActiveTransmissions.Count);
        }
    }
}
