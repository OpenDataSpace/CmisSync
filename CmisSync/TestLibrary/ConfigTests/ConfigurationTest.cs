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

        [Test, Category("Medium")]
        public void TestBrand()
        {
            Uri url = new Uri("http://localhost/cmis/atom");
            string path1 = "/brand/1.png";
            DateTime date1 = DateTime.Now;
            string path2 = "/brand/2.png";
            DateTime date2 = DateTime.Now;

            // Create new config file with default values
            Config config = Config.CreateInitialConfig(this.configPath);

            Assert.IsNull(config.Brand);

            config.Brand = new Brand();
            config.Brand.Server = url;
            config.Brand.Files = new List<BrandFile>();
            BrandFile file1 = new BrandFile();
            file1.Path = path1;
            file1.Date = date1;
            BrandFile file2 = new BrandFile();
            file2.Path = path2;
            file2.Date = date2;
            config.Brand.Files.Add(file1);
            config.Brand.Files.Add(file2);
            config.Save();

            config = Config.CreateOrLoadByPath(this.configPath);
            Assert.AreEqual(url.ToString(), config.Brand.Server.ToString());
            Assert.AreEqual(2, config.Brand.Files.Count);
            Assert.AreEqual(path1, config.Brand.Files[0].Path);
            Assert.AreEqual(date1, config.Brand.Files[0].Date);
            Assert.AreEqual(path2, config.Brand.Files[1].Path);
            Assert.AreEqual(date2, config.Brand.Files[1].Date);
        }
    }
}