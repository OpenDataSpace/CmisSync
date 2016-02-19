//-----------------------------------------------------------------------
// <copyright file="IgnoredFolderNamesFilterTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.FilterTests {
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Events;
    using CmisSync.Lib.Filter;
    using CmisSync.Lib.Storage.FileSystem;

    using Moq;

    using NUnit.Framework;

    [TestFixture, Category("Fast"), Category("EventFilter")]
    public class IgnoredFolderNamesFilterTest {
        private IDirectoryInfo baseDir = Mock.Of<IDirectoryInfo>(dir => dir.FullName == Path.GetFullPath(Path.Combine(Path.GetTempPath(), ".temp")));
        [Test]
        public void ConstructorFailsIfNoBaseDirIsPassed() {
            Assert.Throws<ArgumentNullException>(() => new IgnoredFolderNameFilter(null));
        }

        [Test]
        public void DefaultConstructor() {
            new IgnoredFolderNameFilter(baseDir);
        }

        [Test]
        public void FilterLetsFSEventsPassIfNoWildcardsAreSet() {
            var filter = new IgnoredFolderNameFilter(baseDir);
            string reason;
            Assert.That(filter.CheckFolderName("foldername", out reason), Is.False);
            Assert.That(string.IsNullOrEmpty(reason), Is.True);
        }

        [Test]
        public void FilterTakesWildcardsWithoutFailure() {
            var filter = new IgnoredFolderNameFilter(baseDir);
            var wildcards = new List<string>();
            wildcards.Add("*.tmp");
            filter.Wildcards = wildcards;
        }

        [Test]
        public void FilterTakesEmptyWildcardsWithoutFailure() {
            var filter = new IgnoredFolderNameFilter(baseDir);
            filter.Wildcards = new List<string>();
        }

        [Test]
        public void FilterFailsTakingNullWildcard() {
            var filter = new IgnoredFolderNameFilter(baseDir);
            Assert.Throws<ArgumentNullException>(() => filter.Wildcards = null);
        }

        [Test]
        public void MatchingWildcard() {
            var wildcards = new List<string>();
            wildcards.Add(".*");
            var filter = new IgnoredFolderNameFilter(baseDir) { Wildcards = wildcards };
            string reason;
            Assert.That(filter.CheckFolderName(".test", out reason), Is.True);
            Assert.That(string.IsNullOrEmpty(reason), Is.False);
        }

        [Test]
        public void NoMatchingWildcard() {
            var wildcards = new List<string>();
            wildcards.Add(".tmp");
            var filter = new IgnoredFolderNameFilter(baseDir) { Wildcards = wildcards };
            string reason;
            Assert.That(filter.CheckFolderName(".cache", out reason), Is.False);
            Assert.That(string.IsNullOrEmpty(reason), Is.True);
        }

        [Test]
        public void WildcardInPath() {
            var wildcards = new List<string>();
            wildcards.Add(".*");
            var filter = new IgnoredFolderNameFilter(baseDir) { Wildcards = wildcards };
            string reason;
            var path = Mock.Of<IDirectoryInfo>(dir => dir.FullName == Path.Combine(this.baseDir.FullName, ".test", "allowedPattern"));
            Assert.That(filter.CheckFolderPath(path, out reason), Is.True);
            Assert.That(string.IsNullOrEmpty(reason), Is.False);
        }

        [Test]
        public void WildcardNotInPath() {
            var wildcards = new List<string>();
            wildcards.Add(".*");
            var filter = new IgnoredFolderNameFilter(baseDir) { Wildcards = wildcards };
            string reason;
            var path = Mock.Of<IDirectoryInfo>(dir => dir.FullName == Path.Combine(this.baseDir.FullName, "test", "allowedPattern"));
            Assert.That(filter.CheckFolderPath(path, out reason), Is.False);
            Assert.That(string.IsNullOrEmpty(reason), Is.True);
        }
    }
}