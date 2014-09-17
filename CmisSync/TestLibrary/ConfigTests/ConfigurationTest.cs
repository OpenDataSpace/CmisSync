//-----------------------------------------------------------------------
// <copyright file="ConfigurationTest.cs" company="GRAU DATA AG">
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

namespace TestLibrary.ConfigTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using CmisSync.Lib.Config;

    using NUnit.Framework;

    [TestFixture]
    public class ConfigurationTest
    {
        private readonly string configPath = Path.Combine(Path.GetTempPath(), "testconfig.conf");

        [SetUp, TearDown]
        public void CleanUp() {
            if (File.Exists(this.configPath)) {
                File.Delete(this.configPath);
            }
        }

        [Test, Category("Medium")]
        public void TestConfig()
        {
            // Create new config file with default values
            Config config = Config.CreateInitialConfig(this.configPath);

            // Notifications should be switched on by default
            Assert.IsTrue(config.Notifications);
            Assert.AreEqual(config.Folders.Count, 0);
            Assert.That(config.Log4Net, Is.Not.Null);
            Assert.That(config.GetLog4NetConfig(), Is.Not.Null);
            config.Save();
            config = Config.CreateOrLoadByPath(this.configPath);
        }

        [Test, Category("Medium")]
        public void IgnoreFoldersAreSavedAndLoadedAgain() {
            // Create new config file with default values
            Config config = Config.CreateInitialConfig(this.configPath);
            string ignoreFolderPattern = ".*";
            config.IgnoreFileNames = new List<string>(new string[] { ignoreFolderPattern });
            Assert.That(config.IgnoreFolderNames.Contains(ignoreFolderPattern));
            config.Save();
            config = Config.CreateOrLoadByPath(this.configPath);
            Assert.That(config.IgnoreFolderNames.Contains(ignoreFolderPattern));
        }

        [Test, Category("Medium")]
        public void IgnoreFileNamesAreSavedAndLoadedAgain() {
            // Create new config file with default values
            Config config = Config.CreateInitialConfig(this.configPath);
            string ignoreFileNames = ".*";
            config.IgnoreFileNames = new List<string>(new string[] { ignoreFileNames });
            Assert.That(config.IgnoreFileNames.Contains(ignoreFileNames));
            config.Save();
            config = Config.CreateOrLoadByPath(this.configPath);
            Assert.That(config.IgnoreFileNames.Contains(ignoreFileNames));
        }
    }
}