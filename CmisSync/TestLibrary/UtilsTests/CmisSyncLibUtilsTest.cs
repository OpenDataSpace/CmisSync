﻿using System;
using System.IO;
using CmisSync.Lib;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace TestLibrary.UtilsTests
{
    [TestFixture]
    class CmisSyncLibUtilsTest
    {
        private static readonly string TestFolderParent = Directory.GetCurrentDirectory();
        private static readonly string TestFolder = Path.Combine(TestFolderParent, "conflicttest");

        [SetUp]
        public void TestInit()
        {
            Directory.CreateDirectory(TestFolder);
        }

        [TearDown]
        public void TestCleanup()
        {
            if (Directory.Exists(TestFolder))
            {
                Directory.Delete(TestFolder, true);
            }
        }

        [Test, Category("Fast")]
        public void FindNextFreeFilenameTest()
        {
            string user = "unittest";
            string path = Path.Combine(TestFolder, "testfile.txt");
            string originalParent = Directory.GetParent(path).FullName;
            string conflictFilePath = Utils.FindNextConflictFreeFilename(path, user);
            Assert.AreEqual(path, conflictFilePath, "There is no testfile.txt but another conflict file is created");
            for (int i = 0; i < 10; i++)
            {
                using (FileStream s = File.Create(conflictFilePath)){};
                conflictFilePath = Utils.FindNextConflictFreeFilename(path, user);
                Assert.AreNotEqual(path, conflictFilePath, "The conflict file must differ from original file");
                Assert.True(conflictFilePath.Contains(user), "The username should be added to the conflict file name");
                Assert.True(conflictFilePath.EndsWith(Path.GetExtension(path)), "The file extension must be kept the same as in the original file");
                string filename = Path.GetFileName(conflictFilePath);
                string originalFilename = Path.GetFileNameWithoutExtension(path);
                Assert.True(filename.StartsWith(originalFilename), String.Format("The conflict file \"{0}\" must start with \"{1}\"", filename, originalFilename));
                string conflictParent = Directory.GetParent(conflictFilePath).FullName;
                Assert.AreEqual(originalParent, conflictParent, "The conflict file must exists in the same directory like the orignial file");
            }
        }

        [Test, Category("Fast")]
        public void BandwidthTest()
        {
            long bitPerSecond = 1;
            double bitPerSecondDouble = 1d;
            Assert.AreEqual(Utils.FormatBandwidth(bitPerSecond), Utils.FormatBandwidth(bitPerSecondDouble));
            Assert.True(Utils.FormatBandwidth(bitPerSecond).Contains("1 Bit/s"));
            bitPerSecond = 2;
            bitPerSecondDouble = bitPerSecond;
            Assert.AreEqual(Utils.FormatBandwidth(bitPerSecond), Utils.FormatBandwidth(bitPerSecondDouble));
            Assert.True(Utils.FormatBandwidth(bitPerSecond).Contains("2 Bit/s"));
            bitPerSecond = 1000;
            bitPerSecondDouble = bitPerSecond;
            Assert.AreEqual(Utils.FormatBandwidth(bitPerSecond), Utils.FormatBandwidth(bitPerSecondDouble));
            Assert.True(Utils.FormatBandwidth(bitPerSecond).Contains("1 KBit/s"),Utils.FormatBandwidth(bitPerSecond));
            bitPerSecond = 1100;
            Assert.True(Utils.FormatBandwidth(bitPerSecond).Replace(',','.').Contains("1.1 KBit/s"),Utils.FormatBandwidth(bitPerSecond));
            bitPerSecond = 1499;
            Assert.True(Utils.FormatBandwidth(bitPerSecond).Replace(',','.').Contains("1.5 KBit/s"),Utils.FormatBandwidth(bitPerSecond));
            bitPerSecond = 1500;
            Assert.True(Utils.FormatBandwidth(bitPerSecond).Replace(',','.').Contains("1.5 KBit/s"),Utils.FormatBandwidth(bitPerSecond));
            bitPerSecond = 1000*1000;
            bitPerSecondDouble = bitPerSecond;
            Assert.AreEqual(Utils.FormatBandwidth(bitPerSecond), Utils.FormatBandwidth(bitPerSecondDouble));
            Assert.True(Utils.FormatBandwidth(bitPerSecond).Contains("1 MBit/s"),Utils.FormatBandwidth(bitPerSecond));
            bitPerSecond = 1000*1000*1000;
            bitPerSecondDouble = bitPerSecond;
            Assert.AreEqual(Utils.FormatBandwidth(bitPerSecond), Utils.FormatBandwidth(bitPerSecondDouble));
            Assert.True(Utils.FormatBandwidth(bitPerSecond).Contains("1 GBit/s"),Utils.FormatBandwidth(bitPerSecond));
            bitPerSecond = 1000*1000*1000+100*1000*1000;
            bitPerSecondDouble = bitPerSecond;
            Assert.AreEqual(Utils.FormatBandwidth(bitPerSecond), Utils.FormatBandwidth(bitPerSecondDouble));
            Assert.True(Utils.FormatBandwidth(bitPerSecond).Replace(',','.').Contains("1.1 GBit/s"),Utils.FormatBandwidth(bitPerSecond));
        }

        [Test, Category("Fast")]
        public void FormatIntegerPercentTest()
        {
            int p = 5;
            Assert.AreEqual("5.0 %", Utils.FormatPercent(p).Replace(',','.'));
        }

        [Test, Category("Fast")]
        public void FormatDoublePercentTest()
        {
            double p = 5.03;
            Assert.AreEqual("5.0 %", Utils.FormatPercent(p).Replace(',','.'));
            p = 5.06;
            Assert.AreEqual("5.0 %", Utils.FormatPercent(p).Replace(',','.'));
        }

        [Test, Category("Fast")]
        public void CreateUserAgentTest()
        {
            var useragent = Utils.CreateUserAgent();
            Assert.IsTrue(useragent.Contains(Backend.Version));
            Assert.IsTrue(useragent.Contains("hostname="));
            Assert.IsTrue(useragent.Contains(CultureInfo.CurrentCulture.Name));
//            Console.WriteLine(useragent);
        }
    }
}
