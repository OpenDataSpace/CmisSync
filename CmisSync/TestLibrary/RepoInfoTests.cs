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
            Assert.That(this.info.GetIgnoredPaths(), Is.Empty);
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

