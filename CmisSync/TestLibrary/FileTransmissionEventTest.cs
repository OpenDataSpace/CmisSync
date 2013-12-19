using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CmisSync.Lib.Events;

namespace TestLibrary
{
    using NUnit.Framework;

    [TestFixture]
    public class FileTransmissionEventTest
    {
        private TransmissionProgressEventArgs expectedArgs = null;

        [SetUp]
        public void TestInit()
        {
            expectedArgs = null;
        }

        [Test, Category("Fast")]
        public void ReportProgressTest()
        {
            string filename = "test.txt";
            FileTransmissionEvent transmission = new FileTransmissionEvent(FileTransmissionType.DOWNLOAD_NEW_FILE, filename);
            transmission.TransmissionStatus += TransmissionEventHandler;
            this.expectedArgs = new TransmissionProgressEventArgs() {
                Length = 0,
                ActualPosition = 0,
                BitsPerSecond = 0
            };
            transmission.ReportProgress(this.expectedArgs);
            this.expectedArgs.BitsPerSecond = 1024;
            transmission.ReportProgress(this.expectedArgs);
            this.expectedArgs.Completed = false;
            transmission.ReportProgress(this.expectedArgs);
            this.expectedArgs.Length = 1024;
            transmission.ReportProgress(this.expectedArgs);
            TransmissionProgressEventArgs otherArgs = new TransmissionProgressEventArgs()
            {
                Length = this.expectedArgs.Length,
                ActualPosition = this.expectedArgs.ActualPosition,
                BitsPerSecond = this.expectedArgs.BitsPerSecond,
                Completed = this.expectedArgs.Completed
            };
            transmission.ReportProgress(otherArgs);
        }

        private void TransmissionEventHandler(object sender, TransmissionProgressEventArgs e)
        {
            Assert.AreEqual(expectedArgs, e, "The reported transmission events doesn't fit to the expected ones");
        }

        [Test, Category("Fast")]
        public void CalcBitsPerSecondTest(){
            DateTime start = DateTime.Now;
            DateTime end = start.AddSeconds(1);
            long? BitsPerSecond = TransmissionProgressEventArgs.CalcBitsPerSecond(start, end, 1);
            Assert.AreEqual(8, BitsPerSecond);
            BitsPerSecond = TransmissionProgressEventArgs.CalcBitsPerSecond(start, start, 100);
            Assert.Null(BitsPerSecond);
            BitsPerSecond = TransmissionProgressEventArgs.CalcBitsPerSecond(start, end, 100);
            Assert.AreEqual(8*100, BitsPerSecond);
            try{
                TransmissionProgressEventArgs.CalcBitsPerSecond(end, start, 100);
                Assert.Fail();
            }catch(ArgumentException){}
        }
    }
}
