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

namespace TestLibrary.PathMatcherTests {
    using System;
    using System.IO;

    using CmisSync.Lib.PathMatcher;
    using CmisSync.Lib.Storage.FileSystem;

    using DotCMIS.Client;

    using Moq;

    using NUnit.Framework;

    using TestUtils;

    [TestFixture, Category("Fast")]
    public class PathMatcherTest {
        private string localpath = null;
        private string remotepath = null;

        [SetUp]
        public void SetUp() {
            this.localpath = Path.GetTempPath();
            this.remotepath = "/remote/path/on/server";
        }

        [Test]
        public void ContructorFailsIfLocalPathIsNull([Values(null, "")]string invalidPath) {
            Assert.Throws<ArgumentException>(() => new PathMatcher(invalidPath, this.remotepath));
        }

        [Test]
        public void ConstructorFailsIfBothPathsAreNull(
            [Values(null, "")]string invalidPath,
            [Values(null, "")]string invalidPath2)
        {
            Assert.Throws<ArgumentException>(() => new PathMatcher(invalidPath, invalidPath2));
        }

        [Test]
        public void ConstructorFailsIfRemotePathIsNull([Values(null, "")]string invalidPath) {
            Assert.Throws<ArgumentException>(() => new PathMatcher(this.localpath, invalidPath));
        }

        [Test]
        public void ConstructorTakesLocalAndRemotePath() {
            var underTest = new PathMatcher(this.localpath, this.remotepath);
            Assert.That(underTest.LocalTargetRootPath, Is.EqualTo(this.localpath));
            this.AssertPathEqual(this.remotepath, underTest.RemoteTargetRootPath);
        }

        [Test]
        public void MatchesTest() {
            var underTest = new PathMatcher(this.localpath, this.remotepath);
            Assert.That(underTest.Matches(this.localpath, this.remotepath), Is.True);
            string sameSubfolder = "bla";
            Assert.That(underTest.Matches(Path.Combine(this.localpath, sameSubfolder), this.remotepath + "/" + sameSubfolder), Is.True);
            sameSubfolder = Path.Combine("sub", "folder");
            Assert.That(underTest.Matches(Path.Combine(this.localpath, sameSubfolder), this.remotepath + "/" + sameSubfolder), Is.True);
            string anotherFolder = "another";
            Assert.That(underTest.Matches(Path.Combine(this.localpath, sameSubfolder), this.remotepath + "/" + anotherFolder), Is.False);
            string subfolderOfSame = Path.Combine(sameSubfolder, "sub");
            Assert.That(underTest.Matches(Path.Combine(this.localpath, sameSubfolder), this.remotepath + "/" + subfolderOfSame), Is.False);
            Assert.That(underTest.Matches(Path.Combine(this.localpath, subfolderOfSame), this.remotepath + "/" + sameSubfolder), Is.False);
            string wrongStartingFolder = "wrong";
            Assert.Throws<ArgumentOutOfRangeException>(() => underTest.Matches(Path.Combine(this.localpath, wrongStartingFolder), wrongStartingFolder));
            Assert.Throws<ArgumentOutOfRangeException>(() => underTest.Matches(wrongStartingFolder, wrongStartingFolder));
            Assert.Throws<ArgumentOutOfRangeException>(() => underTest.Matches(wrongStartingFolder, this.remotepath + "/" + wrongStartingFolder));
        }

        [Test]
        public void CanCreateLocalPathTest() {
            string remote = this.remotepath + "/test";
            string wrong = "/wrong/path/on/server/test";
            var underTest = new PathMatcher(this.localpath, this.remotepath);
            Assert.That(underTest.CanCreateLocalPath(this.remotepath), Is.True);
            Assert.That(underTest.CanCreateLocalPath(remote), Is.True);
            Assert.That(underTest.CanCreateLocalPath(wrong), Is.False);
            var remoteFolder = new Mock<IFolder>(MockBehavior.Strict);
            remoteFolder.Setup(f => f.Path).Returns(this.remotepath + "/test2");
            Assert.That(underTest.CanCreateLocalPath(remoteFolder.Object), Is.True);
            var wrongFolder = new Mock<IFolder>(MockBehavior.Strict);
            wrongFolder.Setup(f => f.Path).Returns(wrong + "/test2");
            Assert.That(underTest.CanCreateLocalPath(wrongFolder.Object), Is.False);
        }

        [Test]
        public void CanCreateRemotePathTest() {
            string local = Path.Combine(this.localpath, "test");
            string wrong = Path.Combine("wrong", "path", "on", "client", "test");
            var underTest = new PathMatcher(this.localpath, this.remotepath);
            Assert.That(underTest.CanCreateRemotePath(this.localpath), Is.True);
            Assert.That(underTest.CanCreateRemotePath(local), Is.True);
            Assert.That(underTest.CanCreateRemotePath(wrong), Is.False);
            var localFolder = new DirectoryInfo(Path.Combine(this.localpath, "test2"));
            Assert.That(underTest.CanCreateRemotePath(localFolder), Is.True);
            var wrongFolder = new DirectoryInfo(wrong);
            Assert.That(underTest.CanCreateRemotePath(wrongFolder), Is.False);
        }

        [Test]
        public void CreateLocalPathTest() {
            var underTest = new PathMatcher(this.localpath, this.remotepath);
            string result = underTest.CreateLocalPath(this.remotepath);
            Assert.That(result, Is.EqualTo(this.localpath));
            string subfolder = "sub";
            result = underTest.CreateLocalPath(this.remotepath + "/" + subfolder);
            Assert.That(result, Is.EqualTo(Path.Combine(this.localpath, subfolder)));
            subfolder = "sub/sub";
            result = underTest.CreateLocalPath(this.remotepath + "/" + subfolder);
            Assert.That(result, Is.EqualTo(Path.Combine(this.localpath, "sub", "sub")));
            Assert.Throws<ArgumentOutOfRangeException>(() => underTest.CreateLocalPath("wrong folder"));
        }

        [Test]
        public void CreateRemotePathTest() {
            var underTest = new PathMatcher(this.localpath, this.remotepath);
            string result = underTest.CreateRemotePath(this.localpath);
            this.AssertPathEqual(this.remotepath, result);
            string subfolder = "sub";
            result = underTest.CreateRemotePath(Path.Combine(this.localpath, subfolder));
            Assert.That(result, Is.EqualTo(this.remotepath + "/" + subfolder));
            subfolder = "sub/sub";
            result = underTest.CreateRemotePath(Path.Combine(this.localpath, "sub", "sub"));
            Assert.That(result, Is.EqualTo(this.remotepath + "/" + subfolder));
            Assert.Throws<ArgumentOutOfRangeException>(() => underTest.CreateRemotePath(Path.Combine("wrong", "folder")));
        }

        [Test]
        public void CrossPathCreatingTest() {
            var underTest = new PathMatcher(this.localpath, this.remotepath);
            string result = underTest.CreateRemotePath(this.localpath);
            this.AssertPathEqual(this.remotepath, result);
            result = underTest.CreateLocalPath(result);
            this.AssertPathEqual(this.localpath, result);

            result = underTest.CreateRemotePath(Path.Combine(this.localpath, "sub"));
            result = underTest.CreateLocalPath(result);
            this.AssertPathEqual(Path.Combine(this.localpath, "sub"), result);

            result = underTest.CreateLocalPath(this.remotepath + "/sub");
            result = underTest.CreateRemotePath(result);
            this.AssertPathEqual(this.remotepath + "/sub", result);
        }

        [Test]
        public void GetRelativePath() {
            var underTest = new PathMatcher(this.localpath, this.remotepath);
            string folderName = "new";
            string newLocalPath = Path.Combine(this.localpath, folderName);

            Assert.That(underTest.GetRelativeLocalPath(newLocalPath), Is.EqualTo(folderName));
        }

        [Test]
        public void GetRelativePathDoesNotStartWithSlash() {
            this.localpath = this.localpath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? this.localpath.Substring(0, this.localpath.Length - 1) : this.localpath;
            var underTest = new PathMatcher(this.localpath, "/");
            string folderName = "new";

            Assert.That(underTest.GetRelativeLocalPath(Path.Combine(this.localpath, folderName)).StartsWith(Path.DirectorySeparatorChar.ToString()), Is.False);
        }

        [Test]
        public void RootFolderCanBeRemotelyCreatedWithoutTrailingDenominator() {
            var underTest = new PathMatcher(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar.ToString(), "/");
            Assert.That(underTest.CanCreateRemotePath(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar)), Is.True);
        }

        [Test]
        public void RootFolderCanBeRemotelyCreatedWithTrailingDenominator() {
            var underTest = new PathMatcher(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar), "/");
            Assert.That(underTest.CanCreateRemotePath(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar.ToString()), Is.True);
        }

        [Test]
        public void RootFolderMatchesItselfWithTrailingDenominator() {
            var underTest = new PathMatcher(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar), "/");
            Assert.That(underTest.CanCreateRemotePath(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar.ToString()), Is.True);
        }

        [Test]
        public void RootFolderMatchesItselfWithoutTrailingDenominator() {
            var underTest = new PathMatcher(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar.ToString(), "/");
            Assert.That(underTest.CanCreateRemotePath(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar)), Is.True);
        }

        [Test]
        public void GetRootFolderRelativePathWithoutTrailingDenominator() {
            var underTest = new PathMatcher(Path.GetTempPath(), "/tmp");
            Assert.That(underTest.GetRelativeLocalPath(Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar)), Is.EqualTo("."));
        }

        [Test]
        public void CanCreateLocalPathFailsOnNullPath() {
            var underTest = new PathMatcher(this.localpath, this.remotepath);
            Assert.Throws<ArgumentNullException>(() => underTest.CanCreateLocalPath((string)null));
            Assert.Throws<ArgumentNullException>(() => underTest.CanCreateLocalPath((IFolder)null));
            Assert.Throws<ArgumentNullException>(() => underTest.CanCreateLocalPath((IDocument)null));
        }

        [Test]
        public void CanCreateRemotePathFailsOnNullPath() {
            var underTest = new PathMatcher(this.localpath, this.remotepath);
            Assert.Throws<ArgumentNullException>(() => underTest.CanCreateRemotePath((string)null));
            Assert.Throws<ArgumentNullException>(() => underTest.CanCreateRemotePath((DirectoryInfo)null));
            Assert.Throws<ArgumentNullException>(() => underTest.CanCreateRemotePath((FileInfo)null));
        }

        [Test]
        public void GetRelativeLocalPathFailsOnNullPath() {
            var underTest = new PathMatcher(this.localpath, this.remotepath);
            Assert.Throws<ArgumentNullException>(() => underTest.GetRelativeLocalPath(null));
        }

        [Test]
        public void MatchesFailsOnNullPath() {
            var underTest = new PathMatcher(this.localpath, this.remotepath);
            Assert.Throws<ArgumentNullException>(() => underTest.Matches((string)null, new Mock<IFolder>(MockBehavior.Strict).SetupPath("path").Object));
            Assert.Throws<ArgumentNullException>(() => underTest.Matches((string)null, (IFolder)null));
            Assert.Throws<ArgumentNullException>(() => underTest.Matches((string)null, "ignoreThis"));
            Assert.Throws<ArgumentNullException>(() => underTest.Matches((string)null, (string)null));
            Assert.Throws<ArgumentNullException>(() => underTest.Matches((IDirectoryInfo)null, new Mock<IFolder>(MockBehavior.Strict).Object));
            Assert.Throws<ArgumentNullException>(() => underTest.Matches((IDirectoryInfo)null, (IFolder)null));
            Assert.Throws<ArgumentNullException>(() => underTest.Matches("", (IFolder)null));
            Assert.Throws<ArgumentNullException>(() => underTest.Matches("", (string)null));
            Assert.Throws<ArgumentNullException>(() => underTest.Matches(new Mock<IDirectoryInfo>(MockBehavior.Strict).Object, (IFolder)null));
        }

        private void AssertPathEqual(string left, string right) {
            if (right.EndsWith("/") && !left.EndsWith("/")) {
                Assert.That(right, Is.EqualTo(left + "/"));
            } else if (!right.EndsWith("/") && left.EndsWith("/")) {
                Assert.That(right + "/", Is.EqualTo(left));
            } else if (right.EndsWith("\\") && !left.EndsWith("\\")) {
                Assert.That(right, Is.EqualTo(left + "\\"));
            } else if (!right.EndsWith("\\") && left.EndsWith("\\")) {
                Assert.That(right + "\\", Is.EqualTo(left));
            } else {
                Assert.That(right, Is.EqualTo(left));
            }
        }
    }
}