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
using CmisSync.Lib;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TestLibrary
{
    [TestFixture]
    class ConfigurationTest
    {

        [Test, Category("Slow")]
        public void TestConfig()
        {
            string configpath = Path.GetFullPath("testconfig.conf");
            try
            {
                //Create new config file with default values
                Config config = new Config(configpath);
                //Notifications should be switched on by default
                Assert.IsTrue(config.Notifications);
                Assert.AreEqual(config.Folder.Count, 0);
                config.Save();
                config = new Config(configpath);
            }
            catch (Exception)
            {
                if (File.Exists(configpath))
                    File.Delete(configpath);
                throw;
            }
            File.Delete(configpath);
        }

        [Ignore]
        [Test, Category("Fast")]
        public void IgnoreFoldersTest() {
            Assert.Fail("TODO");
        }
    }
}
