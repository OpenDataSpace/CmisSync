using System;
using System.IO;

using CmisSync.Lib.Data;

using DotCMIS.Client;

using NUnit.Framework;

using Moq;

namespace TestLibrary.DataTests
{
    [TestFixture]
    public class PathMatcherTest
    {

        string localpath = null;
        string remotepath = null;

        [SetUp]
        public void SetUp ()
        {
            localpath = Path.GetTempPath();
            remotepath = "/remote/path/on/server";
        }

        [Test, Category("Fast")]
        public void ConstructorTest ()
        {
            var matcher = new PathMatcher (localpath, remotepath);
            Assert.AreEqual (localpath, matcher.LocalTargetRootPath);
            Assert.AreEqual (remotepath, matcher.RemoteTargetRootPath);
            try {
                new PathMatcher (null, remotepath);
                Assert.Fail ();
            } catch (ArgumentException) {
            }
            try {
                new PathMatcher (null, null);
                Assert.Fail ();
            } catch (ArgumentException) {
            }
            try {
                new PathMatcher (localpath, null);
                Assert.Fail ();
            } catch (ArgumentException) {
            }
            try {
                new PathMatcher (String.Empty, remotepath);
                Assert.Fail ();
            } catch (ArgumentException) {
            }
            try {
                new PathMatcher (localpath, String.Empty);
                Assert.Fail ();
            } catch (ArgumentException) {
            }
        }

        [Test, Category("Fast")]
        public void MatchesTest ()
        {
            var matcher = new PathMatcher (this.localpath, this.remotepath);
            Assert.IsTrue (matcher.Matches (this.localpath, this.remotepath));
            string sameSubfolder = "bla";
            Assert.IsTrue (matcher.Matches (Path.Combine (this.localpath, sameSubfolder), this.remotepath + "/" + sameSubfolder));
            sameSubfolder = Path.Combine ("sub", "folder");
            Assert.IsTrue (matcher.Matches (Path.Combine (this.localpath, sameSubfolder), this.remotepath + "/" + sameSubfolder));
            string anotherFolder = "another";
            Assert.IsFalse (matcher.Matches (Path.Combine (this.localpath, sameSubfolder), this.remotepath + "/" + anotherFolder));
            string subfolderOfSame = Path.Combine (sameSubfolder, "sub");
            Assert.IsFalse (matcher.Matches (Path.Combine (this.localpath, sameSubfolder), this.remotepath + "/" + subfolderOfSame));
            Assert.IsFalse (matcher.Matches (Path.Combine (this.localpath, subfolderOfSame), this.remotepath + "/" + sameSubfolder));
            string wrongStartingFolder = "wrong";
            try {
                matcher.Matches (Path.Combine (this.localpath, wrongStartingFolder), wrongStartingFolder);
                Assert.Fail ("Should throw exception on wrong path start");
            } catch (ArgumentOutOfRangeException) {
            }
            try {
                matcher.Matches (wrongStartingFolder, wrongStartingFolder);
                Assert.Fail ("Should throw exception on wrong path start");
            } catch (ArgumentOutOfRangeException) {
            }
            try {
                matcher.Matches (wrongStartingFolder, this.remotepath + "/" + wrongStartingFolder);
                Assert.Fail ("Should throw exception on wrong path start");
            } catch (ArgumentOutOfRangeException) {
            }
        }

        [Test, Category("Fast")]
        public void CanCreateLocalPathTest ()
        {
            string remote = this.remotepath + "/test";
            string wrong = "/wrong/path/on/server/test";
            var matcher = new PathMatcher (this.localpath, this.remotepath);
            Assert.IsTrue (matcher.CanCreateLocalPath (this.remotepath));
            Assert.IsTrue (matcher.CanCreateLocalPath (remote));
            Assert.IsFalse (matcher.CanCreateLocalPath (wrong));
            var remoteFolder = new Mock<IFolder> ();
            remoteFolder.Setup (f => f.Path).Returns (this.remotepath + "/test2");
            Assert.IsTrue (matcher.CanCreateLocalPath (remoteFolder.Object));
            var wrongFolder = new Mock<IFolder> ();
            wrongFolder.Setup (f => f.Path).Returns (wrong + "/test2");
            Assert.IsFalse (matcher.CanCreateLocalPath (wrongFolder.Object));
        }

        [Test, Category("Fast")]
        public void CanCreateRemotePathTest ()
        {
            string local = Path.Combine(this.localpath , "test");
            string wrong = Path.Combine("wrong","path", "on","client", "test");
            var matcher = new PathMatcher (this.localpath, this.remotepath);
            Assert.IsTrue (matcher.CanCreateRemotePath (this.localpath));
            Assert.IsTrue (matcher.CanCreateRemotePath (local));
            Assert.IsFalse (matcher.CanCreateRemotePath (wrong));
            var localFolder = new DirectoryInfo (Path.Combine(this.localpath, "test2"));
            Assert.IsTrue (matcher.CanCreateRemotePath (localFolder));
            var wrongFolder = new DirectoryInfo (wrong);
            Assert.IsFalse (matcher.CanCreateRemotePath (wrongFolder));
        }

        [Test, Category("Fast")]
        public void CreateLocalPathTest ()
        {
            var matcher = new PathMatcher (this.localpath, this.remotepath);
            string result = matcher.CreateLocalPath (this.remotepath);
            Assert.AreEqual (this.localpath, result);
            string subfolder = "sub";
            result = matcher.CreateLocalPath (this.remotepath + "/" + subfolder);
            Assert.AreEqual (Path.Combine (this.localpath, subfolder), result);
            subfolder = "sub/sub";
            result = matcher.CreateLocalPath (this.remotepath + "/" + subfolder);
            Assert.AreEqual (Path.Combine (this.localpath, "sub", "sub"), result);
            try {
                matcher.CreateLocalPath ("wrong folder");
                Assert.Fail ();
            } catch (ArgumentOutOfRangeException) {
            }
        }

        [Test, Category("Fast")]
        public void CreateRemotePathTest ()
        {
            var matcher = new PathMatcher (this.localpath, this.remotepath);
            string result = matcher.CreateRemotePath (this.localpath);
            Assert.AreEqual (this.remotepath, result);
            string subfolder = "sub";
            result = matcher.CreateRemotePath (Path.Combine (this.localpath, subfolder));
            Assert.AreEqual (this.remotepath + "/" + subfolder, result);
            subfolder = "sub/sub";
            result = matcher.CreateRemotePath (Path.Combine (this.localpath, "sub", "sub"));
            Assert.AreEqual (this.remotepath + "/" + subfolder, result);
            try {
                matcher.CreateRemotePath (Path.Combine("wrong", "folder"));
                Assert.Fail ();
            } catch (ArgumentOutOfRangeException) {
            }
        }

        [Test, Category("Fast")]
        public void CrossPathCreatingTest () {
            var matcher = new PathMatcher (this.localpath, this.remotepath);
            string result = matcher.CreateRemotePath (this.localpath);
            Assert.AreEqual(this.remotepath, result);
            result = matcher.CreateLocalPath(result);
            Assert.AreEqual(this.localpath, result);

            result = matcher.CreateRemotePath ( Path.Combine(this.localpath, "sub"));
            result = matcher.CreateLocalPath (result);
            Assert.AreEqual(Path.Combine(this.localpath, "sub"), result);

            result = matcher.CreateLocalPath ( this.remotepath + "/sub");
            result = matcher.CreateRemotePath (result);
            Assert.AreEqual(this.remotepath + "/sub", result);
        }

        [Test, Category("Fast")]
        public void GetRelativePath()
        {
            var matcher = new PathMatcher(this.localpath, this.remotepath);
            string folderName = "new";
            string newLocalPath = Path.Combine(this.localpath, folderName);

            Assert.That(matcher.GetRelativeLocalPath(newLocalPath), Is.EqualTo(folderName));
        }
    }
}

