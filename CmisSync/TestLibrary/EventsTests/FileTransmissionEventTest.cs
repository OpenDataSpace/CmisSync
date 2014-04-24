//-----------------------------------------------------------------------
// <copyright file="FileTransmissionEventTest.cs" company="GRAU DATA AG">
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CmisSync.Lib.Events;

namespace TestLibrary.EventsTests
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
            TransmissionProgressEventArgs nullArgs = new TransmissionProgressEventArgs()
            {
                Length = null,
                ActualPosition = null,
                BitsPerSecond = null,
                Completed = null
            };
            transmission.ReportProgress(nullArgs);
        }

        private void TransmissionEventHandler(object sender, TransmissionProgressEventArgs e)
        {
            Assert.AreEqual(expectedArgs, e, "The reported transmission events doesn't fit to the expected ones");
            Assert.AreEqual(expectedArgs.BitsPerSecond, e.BitsPerSecond);
        }

        [Test, Category("Fast")]
        public void CalculateBitsPerSecondWithOneMinuteDifference()
        {
            DateTime start = DateTime.Now;
            DateTime end = start.AddMinutes(1);
            long? BitsPerSecond = TransmissionProgressEventArgs.CalcBitsPerSecond(start, end, 1);
            Assert.AreEqual(0, BitsPerSecond);
            BitsPerSecond = TransmissionProgressEventArgs.CalcBitsPerSecond(start, end, 60);
            Assert.AreEqual(8, BitsPerSecond);
        }

        [Test, Category("Fast")]
        public void CalcBitsPerSecondWithOneSecondDifference()
        {
            DateTime start = DateTime.Now;
            DateTime end = start.AddSeconds(1);
            long? BitsPerSecond = TransmissionProgressEventArgs.CalcBitsPerSecond(start, end, 1);
            Assert.AreEqual(8, BitsPerSecond);
            BitsPerSecond = TransmissionProgressEventArgs.CalcBitsPerSecond(start, start, 100);
            Assert.Null(BitsPerSecond);
            BitsPerSecond = TransmissionProgressEventArgs.CalcBitsPerSecond(start, end, 100);
            Assert.AreEqual(8 * 100, BitsPerSecond);
        }

        [Test, Category("Fast")]
        public void CalculateBitsPerSecondWithOneMilisecondDifference()
        {
            DateTime start = DateTime.Now;
            DateTime end = start.AddMilliseconds(1);
            long? BitsPerSecond = TransmissionProgressEventArgs.CalcBitsPerSecond(start, end, 1);
            Assert.AreEqual(8000, BitsPerSecond);
        }

        [Test, Category("Fast")]
        [ExpectedException(typeof(ArgumentException))]
        public void CalculationOfBitsPerSecondFailsOnIllegalDifference()
        {
            DateTime start = DateTime.Now;
            DateTime end = start.AddSeconds(1);
            TransmissionProgressEventArgs.CalcBitsPerSecond(end, start, 100);
        }

        [Test, Category("Fast")]
        public void PercentTest()
        {
            string filename = "test.txt";
            FileTransmissionEvent transmission = new FileTransmissionEvent(FileTransmissionType.DOWNLOAD_NEW_FILE, filename);
            double? percent = null;
            transmission.TransmissionStatus += delegate (object sender, TransmissionProgressEventArgs e) {
                percent = e.Percent;
            };
            transmission.ReportProgress( new TransmissionProgressEventArgs(){});
            Assert.Null(percent);

            this.expectedArgs = new TransmissionProgressEventArgs() {
                Length = 100,
                ActualPosition = 0
            };
            transmission.ReportProgress(this.expectedArgs);
            Assert.AreEqual(0, percent);
            transmission.ReportProgress(new TransmissionProgressEventArgs(){ActualPosition=10});
            Assert.AreEqual(10, percent);
            transmission.ReportProgress(new TransmissionProgressEventArgs(){ActualPosition=100});
            Assert.AreEqual(100, percent);
            transmission.ReportProgress(new TransmissionProgressEventArgs(){Length=1000});
            Assert.AreEqual(10, percent);
            transmission.ReportProgress(new TransmissionProgressEventArgs(){ActualPosition=1000, Length = 2000});
            Assert.AreEqual(50, percent);
        }

        [Test, Category("Fast")]
        public void ConstructorAndPropertiesTest()
        {
            string path = "file";
            FileTransmissionEvent e = new FileTransmissionEvent(FileTransmissionType.DOWNLOAD_MODIFIED_FILE, path);
            Assert.AreEqual (path, e.Path);
            Assert.AreEqual(FileTransmissionType.DOWNLOAD_MODIFIED_FILE, e.Type);
            Assert.IsNull (e.CachePath);
            e = new FileTransmissionEvent(FileTransmissionType.DOWNLOAD_NEW_FILE, path);
            Assert.AreEqual (path, e.Path);
            Assert.AreEqual(FileTransmissionType.DOWNLOAD_NEW_FILE, e.Type);
            Assert.IsNull (e.CachePath);
            e = new FileTransmissionEvent(FileTransmissionType.UPLOAD_MODIFIED_FILE, path);
            Assert.AreEqual (path, e.Path);
            Assert.AreEqual(FileTransmissionType.UPLOAD_MODIFIED_FILE, e.Type);
            Assert.IsNull (e.CachePath);
            e = new FileTransmissionEvent(FileTransmissionType.UPLOAD_NEW_FILE, path);
            Assert.AreEqual (path, e.Path);
            Assert.AreEqual(FileTransmissionType.UPLOAD_NEW_FILE, e.Type);
            Assert.IsNull (e.CachePath);
            string cachepath = "file.sync";
            e = new FileTransmissionEvent(FileTransmissionType.DOWNLOAD_NEW_FILE, path, cachepath);
            Assert.AreEqual (path, e.Path);
            Assert.AreEqual(FileTransmissionType.DOWNLOAD_NEW_FILE, e.Type);
            Assert.AreEqual (cachepath, e.CachePath);
        }
    }
}
