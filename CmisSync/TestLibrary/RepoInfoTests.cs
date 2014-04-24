using System;
using System.IO;

using CmisSync.Lib;

using NUnit.Framework;

using Moq;

namespace TestLibrary.LegacyCodeTests
{
/*    [TestFixture]
    public class RepoInfoTests
    {

    }*/

    [TestFixture]
    public class RepoInfoTests
    {
        private readonly string CMISSYNCDIR = ConfigManager.CurrentConfig.FoldersPath;
        private readonly string ignorePath = "/tmp/test";
        private RepoInfo info;

        [SetUp]
        public void SetUp()
        {
            info = new RepoInfo(
                    "name",
                    CMISSYNCDIR,
                    Path.GetTempPath(),
                    "http://example.com",
                    "user",
                    "password",
                    "",
                    5000);
        }

        [Test, Category("Fast")]
        public void NonAddedPathResultsInEmptyArray()
        {
            Assert.AreEqual(0, info.GetIgnoredPaths().Length);
        }


        [Test, Category("Fast")]
        public void AddIgnorePath()
        {
            info.AddIgnorePath(ignorePath);
            Assert.AreEqual(1, info.GetIgnoredPaths().Length);
            Assert.Contains(ignorePath, info.GetIgnoredPaths());
        }

        [Test, Category("Fast")]
        public void ResetIgnorePaths()
        {
            info.AddIgnorePath(ignorePath);
            info.SetIgnoredPaths(new string[]{});
            Assert.AreEqual(0, info.GetIgnoredPaths().Length);
        }

        [Test, Category("Fast")]
        public void SetIgnorePaths()
        {
            info.SetIgnoredPaths(new string[]{ignorePath});
            Assert.AreEqual(1, info.GetIgnoredPaths().Length);
            Assert.Contains(ignorePath, info.GetIgnoredPaths());
        }

        [Test, Category("Fast")]
        public void IgnoresExactMatchingPath()
        {
            info.AddIgnorePath(ignorePath);
            Assert.IsTrue(info.IsPathIgnored(ignorePath));
        }

        [Test, Category("Fast")]
        public void IgnoreChildOfPath()
        {
            info.AddIgnorePath(ignorePath);
            Assert.IsTrue(info.IsPathIgnored(ignorePath +"/child"), ignorePath +"/child");
        }

        [Test, Category("Fast")]
        public void DoNotIgnorePathWithSameBeginningButNoChildOfIgnore()
        {
            info.AddIgnorePath(ignorePath);
            Assert.IsFalse(info.IsPathIgnored(ignorePath + "stuff"), ignorePath + "stuff");
        }

        [Test, Category("Fast")]
        public void DefaultAuthTypeIsBasicAuthentication()
        {
            Assert.That(info.AuthType, Is.EqualTo(Config.AuthenticationType.BASIC));
        }
    }
}

