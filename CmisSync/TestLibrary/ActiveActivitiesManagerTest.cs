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
            Assert.False(manager.AddTransmission(downloadTransmission), "An already existing file transmission event should be rejected on the second insertion, but wasn't");
            downloadTransmission.ReportProgress(new TransmissionProgressEventArgs() { ActualPosition = 0, Length = 1024 });
            Assert.False(manager.AddTransmission(downloadTransmission), "Transmission is not completed, so the addition of the same event must fail, but didn't");
            downloadTransmission.ReportProgress(new TransmissionProgressEventArgs() { BitsPerSecond = 1024 });
            Assert.False(manager.AddTransmission(downloadTransmission), "Transmission is not completed, so the addition of the same event must fail, but didn't");
            downloadTransmission.ReportProgress(new TransmissionProgressEventArgs() { ActualPosition = 1024 });
            Assert.False(manager.AddTransmission(downloadTransmission), "Transmission is not completed, so the addition of the same event must fail, but didn't");
            downloadTransmission.ReportProgress(new TransmissionProgressEventArgs() { Completed = false });
            Assert.False(manager.AddTransmission(downloadTransmission), "Transmission is not completed, so the addition of the same event must fail, but didn't");


            FileTransmissionEvent downloadModifiedTransmission = new FileTransmissionEvent(downloadModifiedType, downloadfile);
            Assert.True(manager.AddTransmission(downloadModifiedTransmission), "A new modified transmission event should be able to be added on the first time, but wasn't");
            Assert.False(manager.AddTransmission(downloadModifiedTransmission), "An already existing file transmission event should be rejected on the second insertion, but wasn't");
            Assert.False(manager.AddTransmission(downloadModifiedTransmission), "An already existing file transmission event should be rejected on the third insertion, but wasn't");
            downloadModifiedTransmission.ReportProgress(new TransmissionProgressEventArgs() { Completed = true });
            Assert.True(manager.AddTransmission(downloadModifiedTransmission), "A completed file transmission event should be able to be readded after, but wasn't");
            downloadModifiedTransmission.ReportProgress(new TransmissionProgressEventArgs() { Aborted = true });

            downloadTransmission.ReportProgress(new TransmissionProgressEventArgs() { Completed = true });
            Assert.True(manager.AddTransmission(downloadTransmission), "A completed file transmission event should be able to be readded after, but wasn't");
            downloadTransmission.ReportProgress(new TransmissionProgressEventArgs() { Completed = true });

        }
    }
}
