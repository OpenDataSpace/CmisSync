using CmisSync.Lib;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CmisSync.Lib.Config;

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
                Config config = Config.CreateInitialConfig(configpath);
                //Notifications should be switched on by default
                Assert.IsTrue(config.Notifications);
                Assert.AreEqual(config.Folders.Count, 0);
                Assert.That(config.Log4Net, Is.Not.Null);
                Assert.That(config.GetLog4NetConfig(), Is.Not.Null);
                config.Save();
                config = Config.CreateOrLoadByPath(configpath);
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
