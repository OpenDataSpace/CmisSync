//-----------------------------------------------------------------------
// <copyright file="PathMatcherTest.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
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

namespace TestLibrary.DataTests
{
    using System;
    using System.IO;

    using CmisSync.Lib.Data;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class PathMatcherTest
    {
        private string localpath = null;
        private string remotepath = null;

        [SetUp]
        public void SetUp()
        {
            this.localpath = Path.GetTempPath();
            this.remotepath = "/remote/path/on/server";
        }

        [Test, Category("Fast")]
        public void ConstructorTest()
        {
            var matcher = new PathMatcher(this.localpath, this.remotepath);
            Assert.AreEqual(this.localpath, matcher.LocalTargetRootPath);
            AssertPathEqual(this.remotepath, matcher.RemoteTargetRootPath);
            try
            {
                new PathMatcher(null, this.remotepath);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }

            try
            {
                new PathMatcher(null, null);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }

            try
            {
                new PathMatcher(this.localpath, null);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }

            try
            {
                new PathMatcher(string.Empty, this.remotepath);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }

            try
            {
                new PathMatcher(this.localpath, string.Empty);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }
        }

        [Test, Category("Fast")]
        public void MatchesTest()
        {
            var matcher = new PathMatcher(this.localpath, this.remotepath);
            Assert.IsTrue(matcher.Matches(this.localpath, this.remotepath));
            string sameSubfolder = "bla";
            Assert.IsTrue(matcher.Matches(Path.Combine(this.localpath, sameSubfolder), this.remotepath + "/" + sameSubfolder));
            sameSubfolder = Path.Combine("sub", "folder");
            Assert.IsTrue(matcher.Matches(Path.Combine(this.localpath, sameSubfolder), this.remotepath + "/" + sameSubfolder));
            string anotherFolder = "another";
            Assert.IsFalse(matcher.Matches(Path.Combine(this.localpath, sameSubfolder), this.remotepath + "/" + anotherFolder));
            string subfolderOfSame = Path.Combine(sameSubfolder, "sub");
            Assert.IsFalse(matcher.Matches(Path.Combine(this.localpath, sameSubfolder), this.remotepath + "/" + subfolderOfSame));
            Assert.IsFalse(matcher.Matches(Path.Combine(this.localpath, subfolderOfSame), this.remotepath + "/" + sameSubfolder));
            string wrongStartingFolder = "wrong";
            try
            {
                matcher.Matches(Path.Combine(this.localpath, wrongStartingFolder), wrongStartingFolder);
                Assert.Fail("Should throw exception on wrong path start");
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            try
            {
                matcher.Matches(wrongStartingFolder, wrongStartingFolder);
                Assert.Fail("Should throw exception on wrong path start");
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            try
            {
                matcher.Matches(wrongStartingFolder, this.remotepath + "/" + wrongStartingFolder);
                Assert.Fail("Should throw exception on wrong path start");
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        [Test, Category("Fast")]
        public void CanCreateLocalPathTest()
        {
            string remote = this.remotepath + "/test";
            string wrong = "/wrong/path/on/server/test";
            var matcher = new PathMatcher(this.localpath, this.remotepath);
            Assert.IsTrue(matcher.CanCreateLocalPath(this.remotepath));
            Assert.IsTrue(matcher.CanCreateLocalPath(remote));
            Assert.IsFalse(matcher.CanCreateLocalPath(wrong));
            var remoteFolder = new Mock<IFolder>();
            remoteFolder.Setup(f => f.Path).Returns(this.remotepath + "/test2");
            Assert.IsTrue(matcher.CanCreateLocalPath(remoteFolder.Object));
            var wrongFolder = new Mock<IFolder>();
            wrongFolder.Setup(f => f.Path).Returns(wrong + "/test2");
            Assert.IsFalse(matcher.CanCreateLocalPath(wrongFolder.Object));
        }

        [Test, Category("Fast")]
        public void CanCreateRemotePathTest()
        {
            string local = Path.Combine(this.localpath, "test");
            string wrong = Path.Combine("wrong", "path", "on", "client", "test");
            var matcher = new PathMatcher(this.localpath, this.remotepath);
            Assert.IsTrue(matcher.CanCreateRemotePath(this.localpath));
            Assert.IsTrue(matcher.CanCreateRemotePath(local));
            Assert.IsFalse(matcher.CanCreateRemotePath(wrong));
            var localFolder = new DirectoryInfo(Path.Combine(this.localpath, "test2"));
            Assert.IsTrue(matcher.CanCreateRemotePath(localFolder));
            var wrongFolder = new DirectoryInfo(wrong);
            Assert.IsFalse(matcher.CanCreateRemotePath(wrongFolder));
        }

        [Test, Category("Fast")]
        public void CreateLocalPathTest()
        {
            var matcher = new PathMatcher(this.localpath, this.remotepath);
            string result = matcher.CreateLocalPath(this.remotepath);
            Assert.AreEqual(this.localpath, result);
            string subfolder = "sub";
            result = matcher.CreateLocalPath(this.remotepath + "/" + subfolder);
            Assert.AreEqual(Path.Combine(this.localpath, subfolder), result);
            subfolder = "sub/sub";
            result = matcher.CreateLocalPath(this.remotepath + "/" + subfolder);
            Assert.AreEqual(Path.Combine(this.localpath, "sub", "sub"), result);
            try 
            {
                matcher.CreateLocalPath("wrong folder");
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        [Test, Category("Fast")]
        public void CreateRemotePathTest()
        {
            var matcher = new PathMatcher(this.localpath, this.remotepath);
            string result = matcher.CreateRemotePath(this.localpath);
            AssertPathEqual(this.remotepath, result);
            string subfolder = "sub";
            result = matcher.CreateRemotePath(Path.Combine(this.localpath, subfolder));
            Assert.AreEqual(this.remotepath + "/" + subfolder, result);
            subfolder = "sub/sub";
            result = matcher.CreateRemotePath(Path.Combine(this.localpath, "sub", "sub"));
            Assert.AreEqual(this.remotepath + "/" + subfolder, result);
            try
            {
                matcher.CreateRemotePath(Path.Combine("wrong", "folder"));
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }

        [Test, Category("Fast")]
        public void CrossPathCreatingTest()
        {
            var matcher = new PathMatcher(this.localpath, this.remotepath);
            string result = matcher.CreateRemotePath(this.localpath);
            AssertPathEqual(this.remotepath, result);
            result = matcher.CreateLocalPath(result);
            AssertPathEqual(this.localpath, result);

            result = matcher.CreateRemotePath(Path.Combine(this.localpath, "sub"));
            result = matcher.CreateLocalPath(result);
            AssertPathEqual(Path.Combine(this.localpath, "sub"), result);

            result = matcher.CreateLocalPath(this.remotepath + "/sub");
            result = matcher.CreateRemotePath(result);
            AssertPathEqual(this.remotepath + "/sub", result);
        }

        [Test, Category("Fast")]
        public void GetRelativePath()
        {
            var matcher = new PathMatcher(this.localpath, this.remotepath);
            string folderName = "new";
            string newLocalPath = Path.Combine(this.localpath, folderName);

            Assert.That(matcher.GetRelativeLocalPath(newLocalPath), Is.EqualTo(folderName));
        }

        [Test, Category("Fast")]
        public void GetRelativePathDoesNotStartWithSlash()
        {
            this.localpath = this.localpath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? this.localpath.Substring(0, this.localpath.Length - 1) : this.localpath;
            var matcher = new PathMatcher(this.localpath, "/");
            string folderName = "new";

            Assert.That(matcher.GetRelativeLocalPath(Path.Combine(this.localpath, folderName)).StartsWith(Path.DirectorySeparatorChar.ToString()), Is.False);
        }
        
        [Test, Category("Fast")]
        public void RootFolderCanBeRemotelyCreatedWithoutTrailingDenominator()
        {
            var matcher = new PathMatcher(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, "/");

            Assert.That(matcher.CanCreateRemotePath(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar)), Is.True);
        }

        [Test, Category("Fast")]
        public void RootFolderCanBeRemotelyCreatedWithTrailingDenominator()
        {
            var matcher = new PathMatcher(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar), "/");

            Assert.That(matcher.CanCreateRemotePath(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar), Is.True);
        }

        [Test, Category("Fast")]
        public void RootFolderMatchesItselfWithTrailingDenominator()
        {
            var matcher = new PathMatcher(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar), "/");

            Assert.That(matcher.CanCreateRemotePath(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar), Is.True);
        }

        [Test, Category("Fast")]
        public void RootFolderMatchesItselfWithoutTrailingDenominator()
        {
            var matcher = new PathMatcher(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, "/");

            Assert.That(matcher.CanCreateRemotePath(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar)), Is.True);
        }

        [Test, Category("Fast")]
        public void GetRootFolderRelativePathWithoutTrailingDenominator()
        {
            var matcher = new PathMatcher(Path.GetTempPath(), "/tmp");

            Assert.That(matcher.GetRelativeLocalPath(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar)), Is.EqualTo("."));
        }

        private void AssertPathEqual(string left, string right)
        {
            if(right.EndsWith("/") && !left.EndsWith("/")) {
                Assert.That(right, Is.EqualTo(left + "/"));
            } else if(!right.EndsWith("/") && left.EndsWith("/")) {
                Assert.That(right + "/", Is.EqualTo(left));
            } else if(right.EndsWith("\\") && !left.EndsWith("\\")) {
                Assert.That(right, Is.EqualTo(left + "\\"));
            } else if(!right.EndsWith("\\") && left.EndsWith("\\")) {
                Assert.That(right + "\\", Is.EqualTo(left));
            } else {
                Assert.That(right, Is.EqualTo(left));
            }
        }
    }
}
