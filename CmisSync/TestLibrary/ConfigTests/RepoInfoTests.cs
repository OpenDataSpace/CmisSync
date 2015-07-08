//-----------------------------------------------------------------------
// <copyright file="RepoInfoTests.cs" company="GRAU DATA AG">
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

namespace TestLibrary.ConfigTests {
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CmisSync.Lib.Config;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class RepoInfoTests {
        private readonly string cmissyncdir = ConfigManager.CurrentConfig.GetFoldersPath();
        private readonly string ignorePath = "/tmp/test";
        private RepoInfo info;

        [SetUp]
        public void SetUp() {
            this.info = new RepoInfo {
                DisplayName = "name",
                Address = new Uri("http://example.com"),
                User = "user",
                ObfuscatedPassword = new Password("password").ObfuscatedPassword,
                PollInterval = 5000,
                LocalPath = this.cmissyncdir,
                RepositoryId = "repoId",
                RemotePath = "/",
                IgnoredFolders = new List<CmisSync.Lib.Config.RepoInfo.IgnoredFolder>()
            };
        }

        [Test, Category("Fast")]
        public void NonAddedPathResultsInEmptyArray() {
            Assert.That(this.info.GetIgnoredPaths(), Is.Empty);
        }

        [Test, Category("Fast")]
        public void AddIgnorePath() {
            this.info.AddIgnorePath(this.ignorePath);
            Assert.AreEqual(1, this.info.GetIgnoredPaths().Count);
            Assert.Contains(this.ignorePath, this.info.GetIgnoredPaths());
        }

        [Test, Category("Fast")]
        public void ResetIgnorePaths() {
            this.info.AddIgnorePath(this.ignorePath);
            this.info.IgnoredFolders.Clear();
            Assert.AreEqual(0, this.info.GetIgnoredPaths().Count);
        }

        [Test, Category("Fast")]
        public void IgnoresExactMatchingPath() {
            this.info.AddIgnorePath(this.ignorePath);
            Assert.IsTrue(this.info.IsPathIgnored(this.ignorePath));
        }

        [Test, Category("Fast")]
        public void IgnoreChildOfPath() {
            this.info.AddIgnorePath(this.ignorePath);
            Assert.IsTrue(this.info.IsPathIgnored(this.ignorePath + "/child"), this.ignorePath + "/child");
        }

        [Test, Category("Fast")]
        public void DoNotIgnorePathWithSameBeginningButNoChildOfIgnore() {
            this.info.AddIgnorePath(this.ignorePath);
            Assert.IsFalse(this.info.IsPathIgnored(this.ignorePath + "stuff"), this.ignorePath + "stuff");
        }

        [Test, Category("Fast")]
        public void DefaultAuthTypeIsBasicAuthentication() {
            Assert.That(this.info.AuthenticationType, Is.EqualTo(AuthenticationType.BASIC));
        }

        [Test, Category("Fast")]
        public void DefaultConnectionTimeoutIsSet() {
            Assert.That(this.info.ConnectionTimeout, Is.EqualTo(Config.DefaultConnectionTimeout));
        }

        [Test, Category("Fast")]
        public void DefaultReadTimeoutIsSet() {
            Assert.That(this.info.ReadTimeout, Is.EqualTo(Config.DefaultReadTimeout));
        }

        [Test, Category("Fast")]
        public void ConnectionTimeoutTakesPositiveNumber() {
            int positiveNumber = 100;
            this.info.ConnectionTimeout = positiveNumber;
            Assert.That(this.info.ConnectionTimeout, Is.EqualTo(positiveNumber));
        }

        [Test, Category("Fast")]
        public void ConnectionTimeoutTakesZeroOrNegativeNumberAndStoresMinusOne() {
            this.info.ConnectionTimeout = 0;
            Assert.That(this.info.ConnectionTimeout, Is.EqualTo(-1));
            this.info.ConnectionTimeout = -2134;
            Assert.That(this.info.ConnectionTimeout, Is.EqualTo(-1));
        }

        [Test, Category("Fast")]
        public void ReadTimeoutTakesPositiveNumber() {
            int positiveNumber = 100;
            this.info.ReadTimeout = positiveNumber;
            Assert.That(this.info.ReadTimeout, Is.EqualTo(positiveNumber));
        }

        [Test, Category("Fast")]
        public void ReadTimeoutTakesZeroOrNegativeNumberAndStoresMinusOne() {
            this.info.ReadTimeout = 0;
            Assert.That(this.info.ReadTimeout, Is.EqualTo(-1));
            this.info.ReadTimeout = -2134;
            Assert.That(this.info.ReadTimeout, Is.EqualTo(-1));
        }

        [Test, Category("Fast")]
        public void DownloadLimit() {
            long limit = 100;
            Assert.That(this.info.DownloadLimit, Is.EqualTo(0));
            this.info.DownloadLimit = limit;
            Assert.That(this.info.DownloadLimit, Is.EqualTo(limit));
        }

        [Test, Category("Fast")]
        public void UploadLimit() {
            long limit = 100;
            Assert.That(this.info.UploadLimit, Is.EqualTo(0));
            this.info.UploadLimit = limit;
            Assert.That(this.info.UploadLimit, Is.EqualTo(limit));
        }

        [Test, Category("Fast")]
        public void OnSavedTriggersHandler() {
            bool isTriggered = false;
            this.info.Saved += (sender, e) => {
                Assert.That(sender, Is.EqualTo(this.info));
                isTriggered = true;
            };

            this.info.OnSaved();

            Assert.That(isTriggered, Is.True);
        }
    }
}